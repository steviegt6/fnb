**TML.Patcher** is a piece of software designed to aid in the decompilation and unpacking (and eventually patching and re-packing) of `.tmod` files for tModLoader.

See: [Release Notes](RELEASENOTES.md)

## For users:
### Installation Guide
Installation is incredibly simple. Just ensure you meet the prerequisites before following the rest of these steps.

**Prerequisites:**
1. Have .NET 5 installed.
2. Have `ilspycmd` installed (you can install this through the program, if you want).

**Installation:**
1. Grab the newest release from the Releases page.
2. Extract it into an empty folder.
3. Run "`TML.Patcher.Frontend.exe`".
4. (ONLY IF PROMPTED:) Enter in your Mods folder directory. (This should be automatically detected by the program under most circumstances!)

### Troubleshooting
While this program is pretty basic in its operations, it does read and write files, including writing over files and deleting directories in the extraction and decompilation directories. While there is nothing malicious, some anti-virus programs will detect this as suspicious due to this software not being wide-spread, and may prevent you from opening the program or may stop the program from modifying files.

### Reporting Issues
If an issue is found, just report it on the GitHub issue tracker. There's no specific format to follow*, but be sure to be as informative as possible in your reporting.

*For now, there isn't. This may change in the future.

## For developers:
### How TML.Patcher Works:
TML.Patcher is divided into two main libraries, and the frontend console UI is uses an extra library.

Project | Purpose
------- | -------
`TML.Files` | A simple library that provides both generic and tML-specific structs and classes, along with helper/utility methods for handling files. Includes methods for converting `.rawimg`s to `.png`s, simple decompression, etc. Heavily used by the TML.Patcher backend.
`TML.Patcher.Backend` | The core library that provides methods for decompilation and unpacking `.tmod` files. Requires `TML.Files` to use, and `ilspycmd` to decompile extracted mods.
`Consolation` | Console library and "API" that provides a simple way to use the console as a properly-functioning UI. Has a system for dynamic console options as well as launch-parameter support.
`TML.Patcher.Fontend` | Example implementation of `TML.Patcher.Backend`. This is the program you launch when you use the `TML.Patcher` console UI. Also provides a few exclusive functions like listing extracted, installed, and enabled mods.

### Contributing
Contributing is extremely simple. There are no real set standards. Do note that this is developed in .NET 5. Anyone is free to PR new features or resolve known issues. If you aren't sure if something is needed, then contact me and ask.