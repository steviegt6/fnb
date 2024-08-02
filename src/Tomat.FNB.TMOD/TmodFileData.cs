using Tomat.FNB.Common;
using Tomat.FNB.Common.BinaryData;

namespace Tomat.FNB.TMOD;

/// <summary>
///     Minimal information describing the data of a file within a <c>.tmod</c>
///     archive.
/// </summary>
/// <param name="Path">The path of the file within the archive.</param>
/// <param name="Data">The data of the file within the archive.</param>
public readonly record struct TmodFileData(string Path, IDataView Data);