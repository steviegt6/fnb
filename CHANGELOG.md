# Changelog

User-facing changes are documented here, per version.

<!-- ## Unreleased

> **Warning** | This version has not yet been released, and does not reflect the final product nor the current released version. -->

## 2.0.1 - 22 November 2022

### Changes

- Updated `TML.Files` version to 2.1.0 (relevant: (GH-29)[https://github.com/steviegt6/tml-patcher/pull/29]).
  - Noticeable speed improvements as a result ((GH-29)[https://github.com/steviegt6/tml-patcher/pull/29], (@Chik3r)[https://github.com/Chik3r]).

## 2.0.0 - 20 November 2022

Initial release.

### Additions

- Added the `extract` command ("Extracts a .tmod archive file").
  - Added parameter `tmodpath` (0) ("The .tmod archive file to unpack"), takes a string as input.
  - Added option `--output` (`-o`) ("The directory to output to"), determines what directory to output extracted files to.
  - Added option `--threads` (`-t`) ("The amount of threads to use during extraction"), determines how many degrees of parallelism to use during extraction.
- Added the `list` command ("Lists files within a .tmod archive file").
  - Added parameter `tmodpath` (0) ("The .tmod archive file to unpack"), takes a string as input.
- Added the `pack` command ("Packs a .tmod archive file").
  - Added parameter `directory` (0) ("The directory to pack"), takes a string as input.
  - Added option `--output` (`-o`) ("The output file"), determines what to name the resulting `.tmod` archive file.
  - Added option `--mod-loader-version` (`-v`) ("The tModLoader version"), determines what tModLoader version was used to pack this archive (`.tmod` archive file metadata).
  - Added option `--mod-name` (`-n`) ("The mod's internal name"), determines what the mod's internal name is (`.tmod` archive file metadata).
  - Added option `--min-comp-size` (`-c`) ("The minimum size of a file to compress"), determins the minimum length to allow a file to get compressed.
  - Added option `--min-comp-tradeoff` (`-t`) ("The minimum compression tradeoff"), determines the minimum compression tradeoff to allow file compression to pass.
