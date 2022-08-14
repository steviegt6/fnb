using System.Collections.Generic;

namespace TML.Files.Abstractions
{
    /// <summary>
    ///     Represents an object capable of extraction <c>.tmod</c> files.
    /// </summary>
    public interface IModFileExtractor
    {
        /// <summary>
        ///     Extracts the <c>.tmod</c> <paramref name="file"/> into an enumerable collection of <see cref="IExtractedModFile"/> instances.
        /// </summary>
        /// <param name="file">The <c>.tmod</c> file to extract.</param>
        /// <param name="fileExtractors">The objects that should be used to handle extracting files.</param>
        /// <returns>An enumerable collection of extracted files, represented as <see cref="IExtractedModFile"/> instances.</returns>
        IEnumerable<IExtractedModFile> Extract(IModFile file, params IFileExtractor[] fileExtractors);
    }
}