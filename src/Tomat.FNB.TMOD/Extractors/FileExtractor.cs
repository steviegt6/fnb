namespace Tomat.FNB.TMOD.Extractors;

public abstract class FileExtractor {
    public abstract bool ShouldExtract(TmodFile.Entry entry);

    public abstract TmodFile.Data Extract(TmodFile.Entry entry, byte[] data);
}
