using System.IO;

namespace TML.Files.Abstractions
{
    /// <summary>
    ///     Represents an object capable of reading a <c>.tmod</c> file.
    /// </summary>
    public interface IModFileReader
    {
        /// <summary>
        ///     Deserializes a <c>.tmod</c> file from a <paramref name="stream"/> into an <see cref="IModFile"/> instance.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
        /// <returns>An <see cref="IModFile"/> instance.</returns>
        /// <remarks>
        ///     Due to the complex nature of this task, a safe equivalent of this method is not provided. Use try-catch blocks to handle this safely.
        /// </remarks>
        IModFile Read(Stream stream);
    }
}