# TML.Patcher

**TML.Patcher** is a command-line file decompilation, unpacking, and repacking program designed for the modification and reverse-engineering of `.tmod` files for the _tModLoader_ mod loader designed for _Terraria_. Patching support through MonoMod (and Mono.Cecil by extension) using specially-curated software is planned, but recompilation is a possibility as of now, and serves as a working alternative.

With the right thread configuration, the average computer can decompile mods such as _Calamity_ in ~5 seconds, and speeds of even a single second have been reached on higher-end PCs.

### Updates
See: [Release Notes](RELEASENOTES.md)

## For users:
### Installation Guide
Installation is incredibly simple. Just ensure you meet the prerequisites before following the rest of these steps.

**Prerequisites:**
1. .NET 6. Get it [here](https://dotnet.microsoft.com/download/dotnet/6.0).

**Installation:**
1. Grab the newest release from the Releases page.
2. Extract it into an empty folder.
3. Run `dotnet TML.Patcher.CLI.dll` for Unix/\*Nix in your favorite terminal (Windows users can directly execute the `TML.Patcher.CLI.exe` program).
4. (If prompted:) Enter in your Mods folder directory. This is usually auto-detected if you have it on your main drive.

### Troubleshooting
While this program is pretty basic in its operations, it does read and write files, including writing over files and deleting directories in the extraction and decompilation directories.

There is nothing malicious, but some anti-virus programs will detect this as suspicious due to this software not being wide-spread, and may prevent you from opening the program or may stop the program from modifying files.

If this is the case, simply whitelist the program or disable it temporarily. If you don't feel that release builds are safe, you can compile it yourself as well.

### Reporting Issues
If an issue is found, just report it on the GitHub issue tracker. There's no specific format to follow\*, but be sure to be as informative as possible in your reporting.


\*For now, there isn't. This may change in the future.\*


## For developers:
### How TML.Patcher Works:
TML.Patcher is divided into two main libraries, as well as a command-line interface program.

Project | Purpose
------- | -------
`TML.Files` | A simple library that provides both generic and tML-specific structures and and classes, along with helper/utility methods for handling files. Includes methods for converting `.rawimg`s (tML-specific) to `.png`s, simple decompression, etc. Heavily used by the TML.Patcher backend.
`TML.Patcher` | The core library that provides methods for decompilation and unpacking `.tmod` files. Requires `TML.Files` to use.
`TML.Patcher.CLI` | Implementation of `TML.Patcher`. This is the program you launch when you use the `TML.Patcher` console interface. Also provides a few exclusive functions like listing extracted, installed, and enabled mods.

### Nuget
You can get [`TML.Files`](https://www.nuget.org/packages/TML.Files/) and [`TML.Patcher`](https://www.nuget.org/packages/TML.Patcher/) on NuGet.

### Contributing
Contributing is extremely simple. There are no real set standards. Do note that this is developed in .NET 5. Anyone is free to PR new features or resolve known issues. If you aren't sure if something is needed, then contact me and ask.
