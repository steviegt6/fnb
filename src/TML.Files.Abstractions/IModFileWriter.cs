using System.IO;

namespace TML.Files.Abstractions
{
    /// <summary>
    ///     Represents an object capable of writing a <c>.tmod</c> file.
    /// </summary>
    public interface IModFileWriter
    {
        /// <summary>
        ///     Serializes a <c>.tmod</c> file from a <paramref name="file"/> to a <paramref name="stream"/>.
        /// </summary>
        /// <param name="file">The <c>.tmod</c> file to serialize.</param>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <remarks>
        ///     Due to the complex nature of this task, a safe equivalent of this method is not provided. Use try-catch blocks to handle this safely.
        /// </remarks>
        void Write(IModFile file, Stream stream);
    }
}