using Tomat.FNB.Common;
using Tomat.FNB.Common.BinaryData;

namespace Tomat.FNB.TMOD.Extractors;

/// <summary>
///     Defines a file extractor which enables the special handling of certain
///     <c>.tmod</c> file entries.
/// </summary>
public interface ITmodFileExtractor
{
    /// <summary>
    ///     Whether this extractor can extract this entry.
    /// </summary>
    /// <param name="entry">The entry to test for extraction.</param>
    /// <returns>
    ///     Whether this entry can be extracted by this extractor.
    /// </returns>
    bool CanExtract(TmodFileEntry entry);

    /// <summary>
    ///     Extracts an entry from the <c>.tmod</c> archive.
    /// </summary>
    /// <param name="entry">The entry to extract.</param>
    /// <param name="data">
    ///     A direct reference to the data view, which may differ from that of
    ///     the <paramref name="entry"/> if the entry is compressed.
    /// </param>
    /// <returns>The extracted form of the file.</returns>
    TmodFileData Extract(TmodFileEntry entry, IBinaryDataView data);
}
