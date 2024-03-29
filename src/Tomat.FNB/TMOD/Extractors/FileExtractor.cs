﻿using Tomat.FNB.Util;

namespace Tomat.FNB.TMOD.Extractors;

public abstract class FileExtractor {
    public abstract bool ShouldExtract(TmodFileEntry entry);

    public abstract TmodFileData Extract(TmodFileEntry entry, AmbiguousData<byte> data);
}
