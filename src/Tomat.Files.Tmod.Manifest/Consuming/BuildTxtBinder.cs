using System;
using System.Collections.Generic;

namespace Tomat.Files.Tmod.Manifest;

public static class BuildTxtBinder
{
    public static BuildProperties Bind(
        string text,
        List<RawProperty> properties,
        List<Diagnostic> parseDiagnostics
    )
    {
        var result = new BuildProperties();

        result.Diagnostics.AddRange(parseDiagnostics);

        var seen = new HashSet<PropertyKind>();
        var consumer = new DiagnosticsForwarder(result.Diagnostics);

        foreach (var p in properties)
        {
            if (!seen.Add(p.Kind))
            {
                result.Diagnostics.Add(
                    new Diagnostic
                    {
                        Code = DiagnosticCode.DuplicateProperty,
                        Severity = DiagnosticSeverity.Error,
                        Position = p.KeyStart,
                        Message = "Duplicate property",
                        Hint = "Remove duplicate entry",
                    }
                );
                continue;
            }

            switch (p.Kind)
            {
                case PropertyKind.DisplayName:
                    result.DisplayName = Slice(text, p);
                    break;

                case PropertyKind.Author:
                    result.Author = Slice(text, p);
                    break;

                case PropertyKind.Version:
                    if (VersionParser.TryParse(text, p.ValueStart, p.ValueLength, out var parsedVersion, consumer, p.KeyStart))
                    {
                        result.Version = new Version(
                            parsedVersion.Major,
                            Math.Max(parsedVersion.Minor, 0),
                            Math.Max(parsedVersion.Patch, 0),
                            Math.Max(parsedVersion.Revision, 0)
                        );
                    }

                    break;

                case PropertyKind.NoCompile:
                    if (BooleanParser.TryParse(text, p.ValueStart, p.ValueLength, consumer, p.KeyStart, out var noCompile))
                    {
                        result.NoCompile = noCompile;
                    }

                    break;

                case PropertyKind.HideCode:
                    if (BooleanParser.TryParse(text, p.ValueStart, p.ValueLength, consumer, p.KeyStart, out var hideCode))
                    {
                        result.HideCode = hideCode;
                    }

                    break;

                case PropertyKind.HideResources:
                    if (BooleanParser.TryParse(text, p.ValueStart, p.ValueLength, consumer, p.KeyStart, out var hideResources))
                    {
                        result.HideResources = hideResources;
                    }

                    break;

                case PropertyKind.IncludeSource:
                    if (BooleanParser.TryParse(text, p.ValueStart, p.ValueLength, consumer, p.KeyStart, out var includeSource))
                    {
                        result.IncludeSource = includeSource;
                    }

                    break;

                case PropertyKind.Side:
                    if (ModSideParser.TryParse(text, p.ValueStart, p.ValueLength, consumer, p.KeyStart, out var side))
                    {
                        result.Side = side;
                    }

                    break;

                case PropertyKind.ModReferences:
                    ParseModRefs(text, p, result.ModReferences, consumer);
                    break;

                case PropertyKind.SortBefore:
                    ParseStringList(text, p, result.SortBefore, consumer);
                    break;

                case PropertyKind.SortAfter:
                    ParseStringList(text, p, result.SortAfter, consumer);
                    break;

                case PropertyKind.DllReferences:
                    ParseStringList(text, p, result.DllReferences, consumer);
                    break;

                case PropertyKind.WeakReferences:
                    ParseModRefs(text, p, result.WeakReferences, consumer);
                    break;

                case PropertyKind.Homepage:
                    result.Homepage = Slice(text, p);
                    break;

                case PropertyKind.PlayableOnPreview:
                    if (BooleanParser.TryParse(text, p.ValueStart, p.ValueLength, consumer, p.KeyStart, out var playableOnPreview))
                    {
                        result.PlayableOnPreview = playableOnPreview;
                    }

                    break;

                case PropertyKind.TranslationMod:
                    if (BooleanParser.TryParse(text, p.ValueStart, p.ValueLength, consumer, p.KeyStart, out var translationMode))
                    {
                        result.TranslationMod = translationMode;
                    }

                    break;

                case PropertyKind.BuildIgnore:
                    ParseStringList(text, p, result.BuildIgnores, consumer);
                    break;

                case PropertyKind.Unknown:
                    result.Diagnostics.Add(
                        new Diagnostic
                        {
                            Code = DiagnosticCode.UnknownProperty,
                            Severity = DiagnosticSeverity.Error,
                            Position = p.KeyStart,
                            Message = "Unknown property",
                        }
                    );
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return result;
    }

    private static void ParseStringList(
        string text,
        RawProperty p,
        List<string> result,
        IBuildTxtConsumer consumer
    )
    {
        var e = new ValueListEnumerator(text, p.ValueStart, p.ValueLength);

        while (e.MoveNext())
        {
            result.Add(text.Substring(e.CurrentStart, e.CurrentLength));
        }
    }

    private static void ParseModRefs(
        string text,
        RawProperty p,
        List<ModReference> result,
        IBuildTxtConsumer consumer
    )
    {
        var e = new ValueListEnumerator(text, p.ValueStart, p.ValueLength);

        while (e.MoveNext())
        {
            var mod = ModReferenceParser.Parse(text, e.CurrentStart, e.CurrentLength);

            var version = default(Version);
            if (mod.VersionLength > 0 && VersionParser.TryParse(text, mod.VersionStart, mod.VersionLength, out var parsedVersion, consumer, p.KeyStart))
            {
                version = new Version(
                    parsedVersion.Major,
                    Math.Max(parsedVersion.Minor, 0),
                    Math.Max(parsedVersion.Patch, 0),
                    Math.Max(parsedVersion.Revision, 0)
                );
            }

            result.Add(
                new ModReference(
                    text.Substring(mod.NameStart, mod.NameLength),
                    version
                )
            );
        }
    }

    private static string Slice(string text, RawProperty p)
    {
        return text.Substring(p.ValueStart, p.ValueLength);
    }
}
