```c
//------------------------------------------------
//--- 010 Editor v11.0 Binary Template
//
//      File: Sonic Riders Menu Texture Archive
//   Authors: Sewer56
//   Version: 1.0.0
//------------------------------------------------

// Example File: MEXPE -> 40011 -> 00001 (~3 MB)
// Used exclusively with menu archives?

// Utility Struct
struct TextureName
{
    string texName;
};

// Header
short texCount;
short unknown;

// Offset Section
int fileOffsets[texCount];

// Unknown Flags
byte unknownFlagsAlways0x11[texCount];

// Texture Names
TextureName texNames[texCount] <optimize=false>;
```