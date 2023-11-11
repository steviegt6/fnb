using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Tomat.FNB.TMOD.Extractors;

public unsafe partial class FpngExtractor : FileExtractor {
    private class BufSafeHandle : SafeHandleZeroOrMinusOneIsInvalid {
        public BufSafeHandle() : base(true) { }

        protected override bool ReleaseHandle() {
            return fpng_release_image(handle);
        }
    }

    private static bool fpngInitialized;

    public override bool ShouldExtract(TmodFileEntry entry) {
        return Path.GetExtension(entry.Path) == ".rawimg";
    }

    public override TmodFileData Extract(TmodFileEntry entry, byte[] data) {
        if (!fpngInitialized) {
            fpngInitialized = true;
            fpng_init();
        }

        fixed (byte* pData = data) {
            var width = Unsafe.ReadUnaligned<int>(pData + 4);
            var height = Unsafe.ReadUnaligned<int>(pData + 8);
            var rgbaValues = pData + 12;

            EncodeImageWrapper(rgbaValues, width, height, out var image).Dispose();
            return new TmodFileData(Path.ChangeExtension(entry.Path, ".png"), image);   
        }
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
    private static partial bool fpng_release_image(IntPtr bufSafeHandle);

    [LibraryImport("fpng.dll")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial void fpng_init();

    private static unsafe BufSafeHandle EncodeImageWrapper(byte* image, int w, int h, out byte[] memImage) {
        if (!fpng_encode_image_to_memory_wrapper(image, w, h, 4, 0, out var bufHandle, out var imageData, out var length))
            throw new InvalidOperationException();

        memImage = new byte[length];

        for (var i = 0; i < length; i++)
            memImage[i] = imageData[i];

        return bufHandle;
    }
}
