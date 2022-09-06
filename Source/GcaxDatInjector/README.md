# GCAX .DAT Sound Injector

A tool that injects custom sound effects into the GC AX Sound Library (GameCube) variant of DTPK, a format used by SEGA in the early 2000s for storing sound effects. This is not a full unpacker and repacker, but is a hackjob good enough for inserting custom sounds.  

Was made for the GCN version of Sonic Riders in mind, for allowing custom character sound effects.  

## Usage

### Extract a File

Run `GcaxDatInjector.exe extract` with a source file and destination folder.   

`./GcaxDatInjector.exe extract --source path/to/10JSILVR.DAT --destination 10JSILVR-out --convert true`  

Specifying `--convert true` will convert the audio files inside to WAV files; if not specified, they will be extracted RAW.  

### Editing Extracted Files

As this is not a full extractor and repacker; there are limitations:  
- Modified files must use the original sample rate (usually 44.1kHz) and channel count (usually mono) of the files.  

### Injecting into File

Run `GcaxDatInjector.exe pack` with source folder and destination file.  

`GcaxDatInjector.exe inject --source 10JSILVR-out --destination path/to/10JSILVR.DAT`

This tool cannot create new files; only modify existing ones. In this case, `10JSILVR.DAT` must be a file that already exists.  
 

### Patching Game Code

Applications which use DTPK, hardcode the size of the file into the source code; and this size must be modified for the sounds to work properly.  This will vary per game/program.

In the case of Sonic Riders [NTSC-U], if the file uses a recognised file name, the tool will print a gecko code required to use the file, e.g.:

```
045E5BCC 000006C0
045E5BD0 000214D0
```

for 10SONIC.DAT. These gecko codes should work for Vanilla Sonic Riders, Sonic Riders DX (All Versions) and SRTE (1.4.2 and below [2.0+ not supported!!]).  

There is no support for custom characters, only vanilla ones.

### Please Note

I wrote this readme a long time after the tool itself; some details may be a bit fuzzy.

I'm not very comfortable releasing this tool; and I consider having to write it to have been a waste of my time.

To be frank; I consider reverse engineering this format to have been a waste of my time, as another team of people have already done it; but haven't released it to the community after a very long period of time (1+ year). I reversed the format and made my own for the public to use in the meantime; as there's now a good incentive with advent of publicly available fully custom characters. Hope it's useful.

[I totally forgot I made this >w<, sorry!!, should have been public long ago]