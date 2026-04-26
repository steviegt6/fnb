namespace Tomat.Files.Tmod.Manifest;

public enum PropertyKind : byte
{
    Unknown = 0,

    DisplayName,
    Author,
    Version,
    SortBefore,
    SortAfter,
    DllReferences,
    ModReferences,
    WeakReferences,
    Homepage,
    NoCompile,
    HideCode,
    HideResources,
    IncludeSource,
    PlayableOnPreview,
    TranslationMod,
    BuildIgnore,
    Side,
}

internal static class PropertyTable
{
    public static PropertyKind Lookup(string text, int start, int length)
    {
        switch (length)
        {
            case 4:
                if (Match(text, start, "side"))
                {
                    return PropertyKind.Side;
                }

                break;

            case 6:
                if (Match(text, start, "author"))
                {
                    return PropertyKind.Author;
                }

                break;

            case 7:
                if (Match(text, start, "version"))
                {
                    return PropertyKind.Version;
                }

                break;

            case 8:
                if (Match(text, start, "homepage"))
                {
                    return PropertyKind.Homepage;
                }

                if (Match(text, start, "hideCode"))
                {
                    return PropertyKind.HideCode;
                }

                break;

            case 9:
                if (Match(text, start, "sortAfter"))
                {
                    return PropertyKind.SortAfter;
                }

                if (Match(text, start, "noCompile"))
                {
                    return PropertyKind.NoCompile;
                }

                break;

            case 10:
                if (Match(text, start, "sortBefore"))
                {
                    return PropertyKind.SortBefore;
                }

                break;

            case 11:
                if (Match(text, start, "displayName"))
                {
                    return PropertyKind.DisplayName;
                }

                if (Match(text, start, "buildIgnore"))
                {
                    return PropertyKind.BuildIgnore;
                }

                break;

            case 13:
                if (Match(text, start, "dllReferences"))
                {
                    return PropertyKind.DllReferences;
                }

                if (Match(text, start, "modReferences"))
                {
                    return PropertyKind.ModReferences;
                }

                if (Match(text, start, "hideResources"))
                {
                    return PropertyKind.HideResources;
                }

                if (Match(text, start, "includeSource"))
                {
                    return PropertyKind.IncludeSource;
                }

                break;

            case 14:
                if (Match(text, start, "weakReferences"))
                {
                    return PropertyKind.WeakReferences;
                }

                if (Match(text, start, "translationMod"))
                {
                    return PropertyKind.TranslationMod;
                }

                break;

            case 17:
                if (Match(text, start, "playableOnPreview"))
                {
                    return PropertyKind.PlayableOnPreview;
                }

                break;
        }

        return PropertyKind.Unknown;
    }

    private static bool Match(string text, int start, string keyword)
    {
        if (keyword.Length + start > text.Length)
        {
            return false;
        }

        for (var i = 0; i < keyword.Length; i++)
        {
            if (text[start + i] != keyword[i])
            {
                return false;
            }
        }

        return true;
    }
}
