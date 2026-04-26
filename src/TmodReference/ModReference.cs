using System;

namespace Tomat.Files.Tmod;

public readonly record struct ModReference(string Mod, Version? Target)
{
    public override string ToString()
    {
        return Target is null ? Mod : Mod + '@' + Target;
    }

    public static ModReference Parse(string spec)
    {
        var split = spec.Split('@');

        switch (split.Length)
        {
            case 1:
                return new ModReference(split[0], null);

            case > 2:
                throw new Exception("Invalid mod reference: " + spec);

            default:
                try
                {
                    return new ModReference(split[0], new Version(split[1]));
                }
                catch
                {
                    throw new Exception("Invalid mod reference: " + spec);
                }
        }
    }
}
