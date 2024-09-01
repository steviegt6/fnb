// using System.Diagnostics;
// using System.Runtime.Intrinsics;
// using System.Runtime.Intrinsics.X86;
// 
// using static System.Runtime.Intrinsics.X86.Sse2;
// using static System.Runtime.Intrinsics.X86.Sse41;
// 
// namespace Tomat.FNB.FPNG;
// 
// public static unsafe partial class Crc32
// {
//     private static uint SliceBy4(byte* data, nint dataLength, uint curCrc32 = 0)
//     {
//         var crc = ~curCrc32;
//         {
//             var data32 = (uint*)data;
//             for (; dataLength >= sizeof(uint); data32++, dataLength -= 4)
//             {
//                 var v = ReadLe32(data32) ^ crc;
//                 crc = g_crc32_4[0][v >> 24] ^ g_crc32_4[1][(v >> 16) & 0xFF] ^ g_crc32_4[2][(v >> 8) & 0xFF] ^ g_crc32_4[3][v & 0xFF];
//             }
// 
//             for (var data8 = (byte*)data32; dataLength > 0; dataLength--)
//             {
//                 crc = (crc >> 8) ^ (g_crc32_4[0][(crc & 0xFF) ^ *data8++]);
//             }
//         }
//         return ~crc;
//     }
// 
//     // See: "Fast CRC Computation for Generic Polynomials Using PCLMULQDQ
//     //       Instruction"
//     // https://www.intel.com/content/dam/www/public/us/en/documents/white-papers/fast-crc-computation-generic-polynomials-pclmulqdq-paper.pdf
//     // Requires PCLMUL and SSE 4.1; this function skips Step 1 (fold by 4) for
//     // simplicity/less code.
//     private static uint Pclmul(byte* p, nint size, uint crc)
//     {
//         Debug.Assert(size >= 16);
// 
//         // Load first 16 bytes, apply initial CRC32.
//         var b = Xor(ConvertScalarToVector128UInt32(~crc).AsUInt64(), LoadVector128((ulong*)p));
// 
//         // See page 22 (bit-reflected constants for gzip).
//         //   In C# we can directly create Vector128 instances and use them
//         //   instead of handling them here.
//         /*var u = default(Vector128<ulong>);
//         {
//             fixed (ulong* address = &s_u[0])
//             {
//                 u = LoadAlignedVector128(address);
//             }
//         }
//         var k5k0 = default(Vector128<ulong>);
//         {
//             fixed (ulong* address = &s_k5k0[0])
//             {
//                 k5k0 = LoadAlignedVector128(address);
//             }
//         }
//         var k3k4 = default(Vector128<ulong>);
//         {
//             fixed (ulong* address = &s_k3k4[0])
//             {
//                 k3k4 = LoadAlignedVector128(address);
//             }
//         }*/
// 
//         // We're skipping directly to Step 2 page 12 - iteratively folding by 1
//         // (by 4 is overkill for our needs).
//         for (size -= 16, p += 16; size >= 16; size -= 16, p += 16)
//         {
//             // _mm_xor_si128(_mm_clmulepi64_si128(b, k3k4, 17), _mm_loadu_si128(reinterpret_cast<const __m128i*>(p))), _mm_clmulepi64_si128(b, k3k4, 0);
//             b = Xor(Xor(), Pclmulqdq.CarrylessMultiply(b, s_k3k4, 0));
//         }
//     }
// }