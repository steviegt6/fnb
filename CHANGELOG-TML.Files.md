# Changelog

Changelog for the `TML.Files` library.

<!-- ## Unreleased

> **Warning** | This version has not yet been released, and does not reflect the final product nor the current released version. -->

## 2.1.0 - 22 November 2022

### Additions

- Added `System.Threading.Tasks.Dataflow` as a dependency ((GH-29)[https://github.com/steviegt6/tml-patcher/pull/29], (@Chik3r)[https://github.com/Chik3r]).
- Added `void TML.Files.Extraction::Extract(TML.Files.TModFile,int,System.Threading.Tasks.Dataflow.ActionBlock<TML.Files.TModFileData>,TML.Files.Extraction.IFileExtractor)`.
  - See Changes for more information.

### Changes

- Changed `TML.Files.Extraction.TModFileExtractor::Extract` API; you should now interface with extraction using an `System.Threading.Tasks.Dataflow.ActionBlock<TML.Files.TModFileData>` object, allowing you to act immediately when a file is extracted ((GH-29)[https://github.com/steviegt6/tml-patcher/pull/29], (@Chik3r)[https://github.com/Chik3r]).
  - In turn, `System.Collections.Generic.List<TML.Files.TModFileData> TML.Files.Extraction.TModFileExtractor::Extract(TML.Files.TModFile,int,TML.Files.Extraction.IFileExtractor[])` has been made obsolete.
    - In favor of a new method (see Additions).

### Fixes

- Fixed `span.Slice` having the wrong `length` value ((GH-28)[https://github.com/steviegt6/tml-patcher/pull/28], (@Chik3r)[https://github.com/Chik3r]).

## 2.0.0 - 20 November 2022

Initial release.

### Additions

- `.tmod` file serialization.
  - API exporsed through various `TML.Files.TModFileSerializer::Serialize` signatures.
- `.tmod` file deserialization.
  - API exposed through various `TML.Files.TModFileSerializer::Deserialize` signatures.
- `.tmod` file extraction.
  - API exposed through `TML.Files.Extraction.TModFileExtractor::Extract`.
  - Extensibility-based; create your own file extractors by implementing `TML.Files.Extraction.IFileExtractor`.
- `.tmod` file packing.
  - API exposed through various `TML.Files.Extraction.TModFileExtractor::Pack` signatures.
  - Extensibility-based; create your own file extractors by implementing `TML.Files.Extraction.IFilePacker`.
- Useful objects for dealing with `.tmod` files:
  - `TML.Files.TModFile` represents an archive;
  - `TML.Files.TModFileData` represents the raw data needed to add to an archive, created by: instantiating manually, packing, or extracting;
  - `TML.Files.TModFileEntry` represents an actual entry within the `TML.Files.TModFile` (a file within a `.tmod` archive);
  - `TML.Files.Extraction.BuildProperties` represents the data defined within a mod's `build.txt`/`Info` file (ported from tModLoader's codebase);
  - `TML.Files.Extraction.BuildPurpose` represents the reason for which an archive is being packed (unused!);
  - `TML.Files.Extraction.ModReference` is a useful abstraction of the raw string representations of dependencies (includes parsing);
  - and `TML.Files.Extraction.ModSide` represent's a mod's client-server side relation.