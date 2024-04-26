namespace Tomat.FNB.TMOD.Packers;

public abstract class FilePacker {
    public abstract bool ShouldPack(TmodFileData data);

    public abstract TmodFileData Pack(TmodFileData data);
}
