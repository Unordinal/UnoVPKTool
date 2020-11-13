# UnoVPKTool
WIP. A tool for working with Apex Legends VPK files. Currently, it can read all of the file names from a VPK directory file and nothing more.

### Compiling
The WPF app is currently a non-thing.

Compiles for .NET 5.0.

For the [Tests](UnoVPKTool.Tests), a folder named `TestFiles` is required in its directory, with at least `englishclient_frontend.bsp.pak000_dir.vpk` available in it. A few offsets in the test file are hardcoded at the moment, so if the game updates they may stop working.

### Libraries
.NET version of XCompression by Gibbed: https://github.com/gibbed/XCompression
- [License information](XCompression/LICENSE.txt)
