namespace TML.Files.Abstractions
{
    /// <summary>
    ///     Represents an extracted file obtained from an <see cref="IModFileExtractor" /> instance.
    /// </summary>
    public interface IExtractedModFile
    {
        /// <summary>
        ///     The local path, corresponding to the path in the <c>.tmod</c> file.
        /// </summary>
        string LocalPath { get; }
        
        /// <summary>
        ///     The contents of the file, represented as an array of bytes.
        /// </summary>
        byte[] Data { get; }
    }
}