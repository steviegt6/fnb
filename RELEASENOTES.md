# 0.2.1.0
* Abolished `ilspycmd` in exchange for directly using the `ICSharpCode.Decompiler` library.
* Unpacking now opens `.tmod` files with read-only perms, should help with file permission issues.
* Properly support legacy `.tmod` files.

# 0.2.0.1
* Fixed registry issue.
* Fixed path fall-backs.

# 0.2.0.0
* Behind-the-scenes code refactorization.
* Consolation is completely independent.
* TMl.Patcher now uses DragonFruit.
* Backend renamed to TML.Patcher.
* Frontend renamed to TML.Patcher.CLI.
* Uploaded TML.Files and TML.Patcher to NuGet.
* Repackaging mods is now possible.

# 0.1.3.0
* Added light-weight mod unpacking through drag-and-dropping.
* Added the ability to add TML.Patcher.Frontend to the file context menu.

# 0.1.2.0
* Added a logo to the program.
* Added a progress bar with a configurable length.
* Official internal code clean-up and splitting.
* Added One Drive directory detection.
* Added configurable page count.
* Moved release notes and credits to their own page.

# 0.1.1.0
* Considerably quicker extracting speeds.
* Mac & Linux support.
* Behind-the-scenes code clean-up.

# 0.1.0.0
Initial public release. Unpacking, decompilation, mod listings, etc.