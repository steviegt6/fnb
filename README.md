# TML.Patcher
Console application for decompiling, recompiling, packaging, and patching tModLoader's .tmod files.
Currently only capable of unpacking a decompilation. Re-packing and assembly patching (w/ MonoMod) is planned.

## Consolation
Simple console library and "API" for building "flexible" and simple console UIs. Nothing big. Has a ConsoleOption system and Parameter system.

## TML.Files
Core project that contains methods, utilitites, structs, and classes used for file-related tasks in unpacking and decompilation. Has generic and tML-specific contents.

## TML.Patcher.Backend
The core code for the patcher. This contains unpacking and decompilation code and works as a library that allows you can call simple methods for perform the wanted tasks.

## TML.Patcher.Frontend
Example implementation of Consolation & base user interface for TML.Patcher.Backend. Also serves as an example of how to use TML.Patcher.Backend.
