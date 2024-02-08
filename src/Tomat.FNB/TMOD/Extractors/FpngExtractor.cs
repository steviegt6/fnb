using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Tomat.FNB.Util;

namespace Tomat.FNB.TMOD.Extractors;

public unsafe partial class FpngExtractor : FileExtractor {
    private class BufSafeHandle() : SafeHandleZeroOrMinusOneIsInvalid(true) {
        protected override bool ReleaseHandle() {
            return fpng_release_image(handle);
        }
    }

    private static bool fpngInitialized;

    public override bool ShouldExtract(TmodFileEntry entry) {
        return Path.GetExtension(entry.Path) == ".rawimg";
    }

    public override TmodFileData Extract(TmodFileEntry entry, AmbiguousData<byte> data) {
        if (!fpngInitialized) {
            fpngInitialized = true;
            fpng_init();
        }

        var pData = data.Reference;
        var width = Unsafe.ReadUnaligned<int>(pData + 4);
        var height = Unsafe.ReadUnaligned<int>(pData + 8);
        var rgbaValues = pData + 12;

        EncodeImageWrapper(rgbaValues, width, height, out var image).Dispose();
        return new TmodFileData(Path.ChangeExtension(entry.Path, ".png"), image);
    }

    [LibraryImport("fpng.dll")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool fpng_encode_image_to_memory_wrapper(
        void* pImage,
        int width,
        int height,
        int numChannels,
        int flags,
        out BufSafeHandle bufSafeHandle,
        out byte* imageData,
        out int imageLength
    );

    [LibraryImport("fpng.dll")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool fpng_release_image(nint bufSafeHandle);

    [LibraryImport("fpng.dll")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial void fpng_init();

    private static BufSafeHandle EncodeImageWrapper(byte* image, int w, int h, out AmbiguousData<byte> memImage) {
        if (!fpng_encode_image_to_memory_wrapper(image, w, h, 4, 0, out var bufHandle, out var imageData, out var length))
            throw new InvalidOperationException();

        // memImage = new byte[length];
        // for (var i = 0; i < length; i++)
        //     memImage[i] = imageData[i];

        // memImage = new byte[length];
        // Marshal.Copy((nint)imageData, memImage, 0, length);

        // memImage = new Span<byte>(imageData, length).ToArray();

        /*
         *             var destination = new T[_length];
            Buffer.Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref _reference, (uint)_length);
            return destination;
         */

        memImage = new AmbiguousData<byte>(imageData, length);

        return bufHandle;
    }
}
