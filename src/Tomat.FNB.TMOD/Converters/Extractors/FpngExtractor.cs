using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

namespace Tomat.FNB.TMOD.Converters.Extractors;

internal sealed unsafe partial class FpngExtractor : IFileConverter
{
    private sealed class BufSafeHandle() : SafeHandleZeroOrMinusOneIsInvalid(true)
    {
        protected override bool ReleaseHandle()
        {
            return fpng_release_image(handle);
        }
    }

    private static bool initialized;

    public FpngExtractor()
    {
        if (initialized)
        {
            return;
        }

        fpng_init();
        initialized = true;
    }

    bool IFileConverter.ShouldConvert(string path, byte[] data)
    {
        return Path.GetExtension(path) == ".rawimg";
    }

    (string path, byte[] data) IFileConverter.Convert(string path, byte[] data)
    {
        fixed (byte* pData = data)
        {
            var width  = *(int*)(pData + 4);
            var height = *(int*)(pData + 8);
            var pImage = pData + 12;

            EncodeImageWrapper(pImage, width, height, out var imageData).Dispose();
            return (Path.ChangeExtension(path, ".png"), imageData);
        }
    }

    private static BufSafeHandle EncodeImageWrapper(
        byte*      pImage,
        int        width,
        int        height,
        out byte[] imageData
    )
    {
        if (!fpng_encode_image_to_memory_wrapper(pImage, width, height, 4, 0, out var bufHandle, out var pImageData, out var length))
        {
            throw new InvalidOperationException();
        }

        imageData = new byte[length];
        Marshal.Copy((nint)pImageData, imageData, 0, length);
        return bufHandle;
    }

    // ReSharper disable InconsistentNaming
    [LibraryImport("fpng.dll")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool fpng_encode_image_to_memory_wrapper(
        void*             pImage,
        int               width,
        int               height,
        nint              numChannels,
        int               flags,
        out BufSafeHandle bufSafeHandle,
        out byte*         imageData,
        out int           imageLength
    );

    [LibraryImport("fpng.dll")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool fpng_release_image(nint bufSafeHandle);

    [LibraryImport("fpng.dll")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void fpng_init();
    // ReSharper restore InconsistentNaming
}