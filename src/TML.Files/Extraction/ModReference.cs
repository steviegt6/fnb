using System;

namespace TML.Files.Extraction;

public readonly record struct ModReference(string Mod, Version? TargetVersion)
{
    public override string ToString() {
        return TargetVersion is null ? Mod : Mod + '@' + TargetVersion;
    }

    public static ModReference Parse(string value) {
        string[] split = value.Split('@');

        return split.Length switch
        {
            1 => new ModReference(split[0], null),
            2 => new ModReference(split[0], Version.Parse(split[1])),
            _ => throw new ArgumentException("Invalid mod reference", nameof(value))
        };
    }
}