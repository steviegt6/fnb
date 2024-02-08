using System.IO;

namespace Tomat.FNB.Util;

internal static class BinaryWriterExtensions {
    public static void WriteAmbiguousData(this BinaryWriter writer, AmbiguousData<byte> data) {
        writer.Write(data.ToArray());
    }
}
