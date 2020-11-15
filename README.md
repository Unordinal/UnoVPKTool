# UnoVPKTool
WIP. A tool for working with Apex Legends VPK files. Currently, it can read and extract all files with a VPK directory.

### Compiling
The WPF app is currently a non-thing.

Compiles for .NET 5.0.

For the [Tests](UnoVPKTool.Tests), a folder named `TestFiles` is required in its directory, with at least `englishclient_frontend.bsp.pak000_dir.vpk` available in it. A few offsets in the test file are hardcoded at the moment, so if the game updates they may stop working.

### Libraries
Uses a modified version of [Lzham.Net](https://github.com/AndrewSav/Lzham.Net) by AndrewSav.
- [MIT License](LzhamWrapper/LICENSE.txt)
