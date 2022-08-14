namespace TML.Files.Abstractions
{
    /// <summary>
    ///     Handles the extraction of an <see cref="IModFileEntry"/> within an <see cref="IModFile"/>.
    /// </summary>
    public interface IFileExtractor
    {
        /// <summary>
        ///     Determines if this extractor should be used.
        /// </summary>
        /// <param name="fileEntry">The <see cref="IModFileEntry"/> instance that is to be extracted.</param>
        /// <returns>Whether this extractor is applicable to the passed <paramref name="fileEntry"/>.</returns>
        bool ShouldExtract(IModFileEntry fileEntry);

        /// <summary>
        ///     Extracts the passed <paramref name="fileEntry"/> to an <see cref="IExtractedModFile"/> instance.
        /// </summary>
        /// <param name="fileEntry">The <see cref="IModFileEntry"/> to extract.</param>
        /// <param name="data">The uncompressed file data.</param>
        /// <returns>An <see cref="IExtractedModFile"/> instance created from the passed <paramref name="fileEntry"/>.</returns>
        IExtractedModFile Extract(IModFileEntry fileEntry, byte[] data);
    }
}