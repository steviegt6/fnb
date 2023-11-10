namespace Tomat.FNB.TMOD.Extractors;

public sealed class RawByteFileExtractor : FileExtractor {
    public override bool ShouldExtract(TmodFileEntry entry) {
        return true;
    }

    public override TmodFileData Extract(TmodFileEntry entry, byte[] data) {
        return new TmodFileData(entry.Path, data);
    }
}
