using System.Collections.Generic;
using System.IO;

namespace TML.Patcher.Extractors.Info
{
    public readonly record struct InfoKey(string Key, ReadInfoValue Reader)
    {
        public string Key { get; } = Key;

        public ReadInfoValue Reader { get; } = Reader;

        public static InfoKey List(string key) {
            static IEnumerable<string> ReadList(BinaryReader reader) {
                for (string item = reader.ReadString(); item.Length > 0; item = reader.ReadString()) yield return item;
            }

            return new InfoKey(
                key,
                (BinaryReader reader, ref string _, out string? value) => { value = string.Join(", ", ReadList(reader)); }
            );
        }

        public static InfoKey True(string key) {
            return new InfoKey(
                key,
                (BinaryReader reader, ref string _, out string? value) => { value = "true"; }
            );
        }

        public static InfoKey False(string key) {
            return new InfoKey(
                key,
                (BinaryReader reader, ref string tag, out string? value) =>
                {
                    tag = tag.Substring(1);
                    value = "false";
                }
            );
        }

        public static InfoKey Side(string key) {
            return new InfoKey(
                key,
                (BinaryReader reader, ref string tag, out string? value) =>
                {
                    value = reader.ReadByte() switch
                    {
                        0 => "Both",
                        1 => "Client",
                        2 => "Server",
                        3 => "NoSync",
                        _ => null,
                    };
                }
            );
        }

        public static InfoKey Skip(string key) {
            return new InfoKey(key, (BinaryReader reader, ref string _, out string? value) => { value = null; });
        }

        public static InfoKey String(string key) {
            return new InfoKey(
                key,
                (BinaryReader reader, ref string tag, out string? value) => { value = reader.ReadString(); }
            );
        }
    }
}