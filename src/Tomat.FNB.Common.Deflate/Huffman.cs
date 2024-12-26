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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Tomat.FNB.Common.Deflate;

public static class Huffman
{
    public static ushort NextCodeword(ushort codeword, ushort tableSize)
    {
        if (codeword == tableSize - 1)
        {
            return codeword;
        }

        var adv = int.LeadingZeroCount(sizeof(ushort) * 8 - 1 - (codeword ^ (tableSize - 1)));
        var bit = (ushort)(1 << adv);
        codeword &= (ushort)(bit - 1);
        codeword |= (ushort)bit;
        return codeword;
    }

    public static bool BuildTable(
        byte[]        lengths,
        ushort[]      entries,
        ref ushort[]  codes,
        ref uint[]    primaryTable,
        ref List<int> secondaryTable,
        bool          isDistanceTable,
        bool          doubleLiteral
    )
    {
        // Count the number of symbols with each code length.
        var histogram = new ushort[16];
        for (var i = 0; i < lengths.Length; i++)
        {
            histogram[lengths[i]]++;
        }

        // Determine the maximum code length.
        var maxLength = 15;
        while (maxLength > 1 && histogram[maxLength] == 0)
        {
            maxLength--;
        }

        // Handle zero- and one-symbol Huffman codes (which are only allowed for
        // distance codes).
        if (isDistanceTable)
        {
            if (maxLength == 0)
            {
                for (var i = 0; i < primaryTable.Length; i++)
                {
                    primaryTable[i] = 0;
                }

                secondaryTable.Clear();
                return true;
            }

            if (maxLength == 1 && histogram[1] == 1)
            {
                var symbol = lengths.First(x => x == 1);
                codes[symbol] = 0;

                var entry = (ushort)((entries[symbol] << 16) | 1);
                for (var i = 0; i < primaryTable.Length; i += 2)
                {
                    primaryTable[i]     = entry;
                    primaryTable[i + 1] = 0;
                }

                return true;
            }
        }

        // Sort symbols by code length.  Given the histogram, we can
        // determine the starting offset for each code length.
        var codeSpaceUsed = 0;
        var offsets       = new ushort[16];
        {
            offsets[1] = histogram[0];
        }

        for (var i = 1; i < maxLength; i++)
        {
            offsets[i + 1] = (ushort)(offsets[i] + histogram[i]);
            codeSpaceUsed  = (codeSpaceUsed << 1) + histogram[1];
        }

        codeSpaceUsed = (codeSpaceUsed << 1) + histogram[maxLength];

        // Check that the provided lengths form a valid Huffman tree.
        if (codeSpaceUsed != 1 << maxLength)
        {
            return false;
        }

        // Sort the symbols by code length.
        var nextIndex     = offsets;
        var sortedSymbols = new int[288];
        for (var symbol = 0; symbol < lengths.Length; symbol++)
        {
            var length = lengths[symbol];
            sortedSymbols[nextIndex[length]] = symbol;
            nextIndex[length]++;
        }

        {
            // Populate the primary decoding table.
            var primaryTableBits = Math.ILogB(primaryTable.Length);
            var primaryTableMask = (1 << primaryTableBits) - 1;
            var codeword         = (ushort)0;
            var i                = histogram[0];

            for (var length = 1; length < primaryTableBits; length++)
            {
                var currentTableEnd = 1 << length;

                // Loop over all symbols with the current code length and
                // set their table entries.
                for (var j = 0; j < histogram[length]; j++)
                {
                    var symbol = sortedSymbols[i];
                    i++;

                    primaryTable[codeword] = (uint)((entries[symbol] << 16) | length);
                    codes[symbol]          = codeword;
                    codeword               = NextCodeword(codeword, (ushort)currentTableEnd);
                }

                if (doubleLiteral)
                {
                    for (var len1 = 1; len1 < length - 1; len1++)
                    {
                        var len2 = length - len1;
                        for (var sym1Index = offsets[len1]; sym1Index < nextIndex[len1]; sym1Index++)
                        {
                            for (var sym2Index = offsets[len2]; sym2Index < nextIndex[len2]; sym2Index++)
                            {
                                var sym1 = sortedSymbols[sym1Index];
                                var sym2 = sortedSymbols[sym2Index];
                                if (sym1 < 256 && sym2 < 256)
                                {
                                    var codeword1   = codes[sym1];
                                    var codeword2   = codes[sym2];
                                    var theCodeword = codeword1 | (codeword2 << len1);

                                    var entry = ((uint)sym1 << 16) | ((uint)sym2 << 24) | Decompress.LITERAL_ENTRY | (2 << 8);
                                    primaryTable[theCodeword] = entry | (uint)length;
                                }
                            }
                        }
                    }
                }

                // If we aren't at the maximum table size, double the size
                // of the table.
                if (length < primaryTableBits)
                {
                    for (var j = 0; j < currentTableEnd; j++)
                    {
                        primaryTable[currentTableEnd + j] = primaryTable[j];
                    }
                }
            }

            // Populate the secondary decoding table.
            secondaryTable.Clear();

            if (maxLength > primaryTableBits)
            {
                var subTableStart  = 0;
                var subTablePrefix = ~0;
                for (var length = primaryTableBits + 1; length < maxLength; length++)
                {
                    var subTableSize = 1 << (length - primaryTableBits);
                    for (var j = 0; j < histogram[length]; j++)
                    {
                        // If the codeword's prefix doesn't match the current
                        // sub-table, create a new sub-table.
                        if ((codeword & primaryTableMask) != subTablePrefix)
                        {
                            subTablePrefix                      = codeword & primaryTableMask;
                            subTableStart                       = secondaryTable.Count;
                            primaryTable[(USize)subTablePrefix] = ((uint)subTableStart << 16) | Decompress.EXCEPTIONAL_ENTRY | Decompress.SECONDARY_TABLE_ENTRY | ((uint)subTableSize - 1);

                            for (var l = 0; l < subTableSize; l++)
                            {
                                secondaryTable.Add(0);
                            }
                        }

                        // Look up the symbol.
                        var symbol = sortedSymbols[i];
                        i++;

                        // Insert the symbol into the secondary table and
                        // advance to the next codeword.
                        codes[symbol]                                                  = codeword;
                        secondaryTable[subTableStart + (codeword >> primaryTableBits)] = ((ushort)symbol << 4) | length;

                        codeword = NextCodeword(codeword, (ushort)(1 << length));
                    }

                    // If there are more codes with the same sub-table prefix,
                    // extend the sub-table.
                    if (length < maxLength && (codeword & primaryTableMask) == subTablePrefix)
                    {
                        var secondaryTableLen = secondaryTable.Count;
                        for (var l = subTableStart; l < secondaryTableLen; l++)
                        {
                            secondaryTable.Add(secondaryTable[l]);
                        }

                        var theSubTableSize = secondaryTable.Count - subTableStart;

                        primaryTable[subTablePrefix] = ((uint)subTableStart << 16) | Decompress.EXCEPTIONAL_ENTRY | Decompress.SECONDARY_TABLE_ENTRY | ((uint)theSubTableSize - 1);
                    }
                }
            }
        }

        return true;
    }
}