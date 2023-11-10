using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Tomat.FNB.TMOD.Extractors; 

public class FpngExtractor : FileExtractor {
    private static bool _fpngInitialized = false;
    
    public override bool ShouldExtract(TmodFileEntry entry) {
        return Path.GetExtension(entry.Path) == ".rawimg";
    }

    public override TmodFileData Extract(TmodFileEntry entry, byte[] data) {
        if (!_fpngInitialized) {
            _fpngInitialized = true;
            fpng_init();
        }
        
        ReadOnlySpan<byte> span = data;
        int width = MemoryMarshal.Read<int>(span.Slice(4, 4));
        int height = MemoryMarshal.Read<int>(span.Slice(8, 4));
        Memory<byte> rgbaValues = data.AsMemory(12);

        // using Image<Rgba32> image = Image.WrapMemory<Rgba32>(Configuration.Default, rgbaValues, width, height);
        EncodeImageWrapper(rgbaValues, width, height, out byte[] image).Dispose();
        return new TmodFileData(Path.ChangeExtension(entry.Path, ".png"), image);
    }



    [DllImport("fpng.dll", CallingConvention = CallingConvention.Cdecl)]
    static unsafe extern bool fpng_encode_image_to_memory_wrapper(void* pImage, int w, int h, int num_chans, int flags,
        out BufSafeHandle bufSafeHandle, out byte* imageData, out int imageLength);
    
    [DllImport("fpng.dll", CallingConvention = CallingConvention.Cdecl)]
    static unsafe extern bool fpng_release_image(IntPtr bufSafeHandle);
    
    [DllImport("fpng.dll", CallingConvention = CallingConvention.Cdecl)]
    static unsafe extern void fpng_init();

    static unsafe BufSafeHandle EncodeImageWrapper(Memory<byte> image, int w, int h, out byte[] memImage) {
        BufSafeHandle bufHandle;
        var pinnedImage = image.Pin();
        if (!fpng_encode_image_to_memory_wrapper(pinnedImage.Pointer, w, h, 4, 0, out bufHandle, out byte* imageData, out int length)) {
            throw new InvalidOperationException();
        }

        memImage = new byte[length];
        for (int i = 0; i < length; i++) {
            memImage[i] = imageData[i];
        }

        return bufHandle;
    }
    
    class BufSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public BufSafeHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return fpng_release_image(handle);
        }
    }

}