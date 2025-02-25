#region License
/*
 * Copyright (c)  2024 Tomat et al.           <https://github.com/steviegt6/fnb>
 * Copyright (c)  2024 Jonathan Behrens   <https://github.com/image-rs/fdeflate>
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
#endregion

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Linq;

namespace Tomat.FNB.Common.Deflate;

internal static class Util
{
    /// <summary>
    ///     Build a length-limited huffman tree.
    ///     <br />
    ///     Dynamic programming algorithm from fpnge.
    /// </summary>
    public static unsafe void ComputeCodeLengths(ulong[] freqs, byte[] minLimit, byte[] maxLimit, byte[] calculatedNBits)
    {
        Debug.Assert(freqs.Length == minLimit.Length);
        Debug.Assert(freqs.Length == maxLimit.Length);
        Debug.Assert(freqs.Length == calculatedNBits.Length);

        var len = freqs.Length;
        for (var i = 0; i < len; i++)
        {
            Debug.Assert(minLimit[i] >= 1 && minLimit[i] <= maxLimit[i]);
        }

        var precision   = maxLimit.Max();
        var numPatterns = 1 << precision;

        var dynp = new ulong[(numPatterns + 1) * (len + 1)];

        dynp[Index(0, 0, numPatterns)] = 0;
        for (var sym = 0u; sym < len; sym++)
        {
            for (var bits = minLimit[sym]; bits <= maxLimit[sym]; bits++)
            {
                var offDelta = 1u << (precision - bits);

                for (var off = 0u; off < uint.CreateSaturating(numPatterns - offDelta); off++)
                {
                    fixed (byte* pBit = &minLimit[sym])
                    {
                        dynp[Index(sym + 1, off + offDelta, numPatterns)] = Math.Min(
                            ulong.CreateSaturating(dynp[Index(sym, off, numPatterns)] + freqs[sym] * BitConverter.ToUInt64(new ReadOnlySpan<byte>(pBit, sizeof(ulong)))),
                            dynp[Index(sym + 1, off + offDelta, numPatterns)]
                        );
                    }
                }
            }
        }

        {
            var sym = (uint)len;
            var off = (uint)numPatterns;

            while (sym > 0)
            {
                sym--;
                Debug.Assert(off > 0);

                for (var bits = minLimit[sym]; bits <= maxLimit[sym]; bits++)
                {
                    var offDelta = 1u << (precision - bits);

                    fixed (byte* pBit = &minLimit[sym])
                    {
                        if (offDelta <= off && dynp[Index(sym + 1, off, numPatterns)] == ulong.CreateSaturating(dynp[Index(sym, off - offDelta, numPatterns)]) + freqs[sym] * BitConverter.ToUInt64(new ReadOnlySpan<byte>(pBit, sizeof(ulong))))
                        {
                            off                  -= offDelta;
                            calculatedNBits[sym] =  bits;
                            break;
                        }
                    }
                }
            }

            for (var i = 0; i < len; i++)
            {
                Debug.Assert(calculatedNBits[i] >= minLimit[i] && calculatedNBits[i] <= maxLimit[i]);
            }
        }
        return;

        static long Index(USize sym, USize off, int numPatterns)
        {
            return sym * (numPatterns + 1) + off;
        }
    }

    public static ushort[] ComputeCodes(byte[] lengths)
    {
        var codes = new ushort[lengths.Length];
        var code  = 0u;

        for (var len = 1; len <= 16; len++)
        {
            for (var i = 0; i < lengths.Length; i++)
            {
                if (lengths[i] != len)
                {
                    continue;
                }

                codes[i] = (ushort)(BinaryPrimitives.ReverseEndianness((ushort)code) >> (16 - len));
                code++;
            }

            code <<= 1;
        }

        if (code == 2 << 16)
        {
            return codes;
        }

        throw new InvalidOperationException("Invalid code lengths");
    }
}