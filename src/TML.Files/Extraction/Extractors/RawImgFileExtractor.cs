using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TML.Files.Extraction.Extractors
{
    /// <summary>
    ///     Extracts a .tmod <c>.rawimg</c> file to a <c>.png</c> file.
    /// </summary>
    public class RawImgFileExtractor : IFileExtractor
    {
        /// <inheritdoc cref="IFileExtractor.ShouldExtract"/>
        public bool ShouldExtract(TModFileEntry entry) {
            return Path.GetExtension(entry.Path) == ".rawimg";
        }

        /// <inheritdoc cref="IFileExtractor.Extract"/>
        public TModFileData Extract(TModFileEntry entry, byte[] data) {
            // TODO: optimize this a ton
            ReadOnlySpan<byte> span = data;
            int width = MemoryMarshal.Read<int>(span.Slice(4, 8));
            int height = MemoryMarshal.Read<int>(span.Slice(8, 12));
            Memory<byte> rgbaValues = data.AsMemory(12);

            using Image<Rgba32> image = Image.WrapMemory<Rgba32>(Configuration.Default, rgbaValues, width, height);

            using MemoryStream stream = new();
            image.SaveAsPng(stream);
            return new TModFileData(Path.ChangeExtension(entry.Path, ".png"), stream.ToArray());
        }
    }
}