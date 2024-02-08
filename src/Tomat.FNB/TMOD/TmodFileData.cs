using Tomat.FNB.Util;

namespace Tomat.FNB.TMOD;

public readonly record struct TmodFileData(string Path, AmbiguousData<byte> Data);
