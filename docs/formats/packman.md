[Work in Progress]

## Data Structures

### Archives (1, PA00, EVDM etc.)
- [Archive Format](./template/PackMan.md)
- [Menu Texture Format (40011/00001)](./template/MenuTextureArchive.md)

### Stage Archives (1, 1M, 1V etc.)
Brackets indicate source file in the format of `id/fileNo` as extracted by the archive tool.

- [Object Layout Culling Format (00301/00000)](./template/ObjectVisibility.md)
- [Object Layout Format (00305/00000)](./template/ObjectLayout.md)

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