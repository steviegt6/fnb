using System.IO;
using System.Runtime.InteropServices;

namespace Tomat.FNB.TMOD.Converters.Extractors;

internal sealed unsafe partial class ImageFnbExtractor : IFileConverter
{
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

            // var image     = create_image_from_raw_data(width, height, pImage);
            // var pPng      = encode_png(image, out var length);
            // var imageData = new byte[length];
            // Marshal.Copy((nint)pPng, imageData, 0, (int)length);
            // free_encoded_png(pPng);
            // free_decoded_image(image);
            // return (Path.ChangeExtension(path, ".png"), imageData);

            var pPng      = encode_png(width, height, pImage, out var length);
            var imageData = new byte[length];
            Marshal.Copy((nint)pPng, imageData, 0, (int)length);
            free_encoded_png(pPng);
            return (Path.ChangeExtension(path, ".png"), imageData);
        }
    }

#region image_fnb
    // [LibraryImport("image_fnb", EntryPoint = "decode_png")]
    // private static partial nint decode_png(byte* data, nint length);

    // [LibraryImport("image_fnb", EntryPoint = "encode_png")]
    // private static partial byte* encode_png(nint image, out nint length);

    // [LibraryImport("image_fnb", EntryPoint = "create_image_from_raw_data")]
    // private static partial nint create_image_from_raw_data(nint width, nint height, byte* data);

    // [LibraryImport("image_fnb", EntryPoint = "free_decoded_image")]
    // private static partial void free_decoded_image(nint image);

    // [LibraryImport("image_fnb", EntryPoint = "free_encoded_png")]
    // private static partial void free_encoded_png(byte* data);

    [LibraryImport("image_fnb", EntryPoint = "encode_png")]
    private static partial byte* encode_png(nint width, nint height, byte* data, out nint length);

    [LibraryImport("image_fnb", EntryPoint = "free_encoded_png")]
    private static partial void free_encoded_png(byte* data);
#endregion
}