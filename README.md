<div align="center">
	<h1>Sonic Riders PC Wiki</h1>
	<img src="./Images/Icon.png" Width=200 /><br/>
	<strong>🎈 Let's get hacking 🎈</strong>
    <p>A repository of my personal tools and info for modding Riders</p>
</div>

# Index

## Tools

- Archive Packer/Unpacker
- Object Layout Editor

## Guides

- Porting Objects Across Stages

## Data Structures

### Archives (1, PA00, EVDM etc.)
- [Archive Format](./Templates/RidersPackMan.bt)

### Stage Archives (1, 1M, 1V etc.)
Brackets indicate source file in the format of `id/fileNo` as extracted by the archive tool.

- [Object Layout Culling Format (00301/00000)](./Templates/RidersObjectPortals.bt)
- [Object Layout Format (00305/00000)](./Templates/RidersObjectLayout.bt)

#### ID Lists
Semi-Optional: File can be empty but needs to be present.

- Object Layout Culling a.k.a. "Map Portal" (00301) [Optional]
- Unknown Small Collection of SEGA NN Files (00302) [??]
- Object Layout [Test Level Only?] (00300/00000) [Required: 8 Player objects (id 0) are required at minimum.]
- Object Layout (00305/00000) [Required: 8 Player objects (id 0) are required at minimum.]
- Object Layout: Lighting (00305/00001) [Semi-Optional]
- Object Layout: Fog (00305/00002) [Semi-Optional]
  
- Effect Objects (01XXX) [Lens flare, etc. Optional: Only if referenced in object layout.]
- Common Objects (02XXX) [Optional: Only if referenced in object layout. Except 2100.]
- Stage Specific Objects (05XXX) [Required if referenced in object layout]
- Common Special Effects & Particle Emitters? (31XXX) [Required]
- Particle Presets (33XXX) [Optional]
- Mission Mode Objects (41XXX) [Optional]
- Survival Objects (48XXX) [Optional]

# Prerequisites
- **.NET 5:** For compiling the tools.

If you are not familiar with C# or the .NET platform, I would also recommend installing **Visual Studio** for development.

# Credits, Attributions

- Header icon made by <a href="https://www.flaticon.com/authors/prosymbols" title="Prosymbols">Prosymbols</a> from <a href="https://www.flaticon.com/" title="Flaticon"> www.flaticon.com</a>