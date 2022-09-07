using CommandLine;

namespace RidersTextureArchiveTool
{
    [Verb("extract", HelpText = "Extracts a Riders Texture Archive file.")]
    internal class ExtractOptions
    {
        [Option(Required = true, HelpText = "The archive file to extract.")]
        public string Source { get; private set; }

        [Option(Required = true, HelpText = "The folder to extract files to.")]
        public string SavePath { get; private set; }

        [Option(Required = false, HelpText = "Set to true if file uses Big Endian.", Default = false)]
        public bool BigEndian { get; private set; }
    }


    [Verb("pack", HelpText = "Packs a Riders Texture Archive file.")]
    internal class PackOptions
    {
        [Option(Required = true, HelpText = "The folder containing the files to pack.")]
        public string Source { get; internal set; }

        [Option(Required = true, HelpText = "The path to which to save the new archive.")]
        public string SavePath { get; internal set; }

        [Option(Required = false, HelpText = "Set to true if file uses Big Endian.", Default = false)]
        public bool BigEndian { get; internal set; }

        [Option(Required = false, HelpText = "Emits unknown flags.", Default = true)]
        public bool? EmitUnknownFlags { get; internal set; }
    }

    [Verb("packall", HelpText = "Packs a list of Riders Texture Archive files.")]
    internal class PackAllOptions
    {
        [Option(Required = true, HelpText = "Text file containing folders with the files to pack in the same format as extracted. Each folder should be on a separate line.")]
        public string Sources { get; private set; }

        [Option(Required = true, HelpText = "Text file containing paths where the new archives should be saved. Each file should be on a separate line.")]
        public string SavePaths { get; private set; }

        [Option(Required = false, HelpText = "Set to true if file uses Big Endian.", Default = false)]
        public bool BigEndian { get; internal set; }
    }
}
