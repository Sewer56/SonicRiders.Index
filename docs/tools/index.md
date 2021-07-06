# Tools 

Please refer to the [formats page](../files/formats.md) for more information about each of the formats.

| Extension   | Format | Tool | Description |
| ----------- | ------ | ---- | ----------- |
| .DAT | Audio Archive: XBOX DTPK | [DTPKUtil](https://github.com/Shenmue-Mods/DTPKUtil) | Doesn't support the format variation used in Riders, but may some day. |
| N/A | Archive: Riders Archive | [RidersArchiveTool](https://github.com/Sewer56/SonicRiders.Index/releases) | Flawlessly extracts and repacks archives with 1:1 output. |
| N/A | Archive: Riders Archive | [Sega NN Tools](https://github.com/Argx2121/Sega_NN_tools) | Transparently unpacks from the format to search for models. |
| .ADX | Audio: CRI ADX | [radx](https://github.com/Isaac-Lozano/radx) | The (only?) open source ADX Encoder |
| .ADX | Audio: CRI ADX | AtomENCD | GUI for adxencd. [Here's a guide on how to use it](https://gamebanana.com/tuts/13109) |
| .ADX | Audio: CRI ADX | [adxencd](http://shenmuesubs.sourceforge.net/download/addons/CRI_Middleware_ADX_Tools_v1.15_Linux_Win32.rar) | The official ADX Encoder from the CRI SDK. |
| .AIX | Audio: CRI AIX | [aixmaker (CRI SDK)](http://shenmuesubs.sourceforge.net/download/addons/CRI_Middleware_ADX_Tools_v1.15_Linux_Win32.rar) | The official AIX Encoder from the CRI SDK. |
| .AFS | Audio Container: CRI AFS | [AFSUtils](https://sourceforge.net/projects/shenmuesubs/files/AFS%20Utils/afsutils_23.7z/download) | Yet another AFS packer/unpacker. |
| .AFS | Audio Container: CRI AFS | [AFS Explorer](https://www.moddingway.com/file/270.html) | Ancient tool intended for PES games; but still commonly used. |
| .SFD | Video: CRI SofDec | [SofDec CRAFT (CRI SDK)](https://archive.org/details/CRISDK113XB) | Official tool from CRI SDK for creating SofDec videos. |
| .SFD | Video: CRI SofDec | [SfdMux](https://archive.org/details/CRISDK113XB) | Creates SFD files from MPEG 1 Video and Audio. [Tutorial](https://gamebanana.com/tuts/13387) |
| .XNO | Graphics: Sega NN: 3D Object | [Sega NN Tools](https://github.com/Argx2121/Sega_NN_tools) | Supports PC Riders models, either through direct file open or by searching them in Riders archives. Does not export custom models (yet). |
| .XVRS | Texture Archive: PVR Texture Library | [Sega NN Tools](https://github.com/Argx2121/Sega_NN_tools) | Can directly replace textures and convert them to DDS. |
| .XVRS | Texture Archive: PVR Texture Library | [RidersTextureArchiveTool](https://github.com/Sewer56/SonicRiders.Index/releases) | Only unpacks/repacks the archive container. Does not yet support conversion of textures to DDS. |

## Other Tools

- **IndexTool**: [Available in this repository](https://github.com/Sewer56/SonicRiders.Index/releases). Can automatically generate documentation and recognize many file types.

## Building the Tools
Some of the tools mentioned are contained in this repository.

To compile them, you will need the following:  
- **.NET 5 SDK**  

To compile them, simply go to the `Source` folder and run the `BuildTools.ps1` script in Powershell.

If you wish to contribute to the tools and are not familiar with C# or the .NET platform, I would recommend installing **Visual Studio** for development.