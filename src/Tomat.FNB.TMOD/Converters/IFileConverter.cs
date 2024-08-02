namespace Tomat.FNB.TMOD.Converters;

public interface IFileConverter
{
    bool ShouldConvert(string path, byte[] data);

    (string path, byte[] data) Convert(string path, byte[] data);
}