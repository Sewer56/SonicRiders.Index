using CommandLine;

namespace RidersArchiveTool
{
    [Verb("extract", HelpText = "Extracts a Riders Archive file.")]
    internal class ExtractOptions
    {
        [Option(Required = true, HelpText = "The archive file to extract.")]
        public string Source { get; internal set; }

        [Option(Required = true, HelpText = "The folder to extract files to.")]
        public string SavePath { get; internal set; }

        [Option(Required = false, HelpText = "Set to true if file uses Big Endian.", Default = false)]
        public bool BigEndian { get; internal set; }

        [Option(Required = false, HelpText = "Does not print out files written to disk.", Default = false)]
        public bool Silent { get; internal set; }
    }

    [Verb("extractall", HelpText = "Extracts all Riders archives in a given folder.")]
    internal class ExtractAllOptions
    {
        [Option(Required = true, HelpText = "The folder containing archives to extract.")]
        public string Source { get; private set; }

        [Option(Required = true, HelpText = "The folder to save the extracted archives to.")]
        public string SavePath { get; private set; }

        [Option(Required = false, HelpText = "Set to true if file uses Big Endian.", Default = false)]
        public bool BigEndian { get; private set; }
    }

    [Verb("packall", HelpText = "Packs all folders in a given path into Riders archive files.")]
    internal class PackAllOptions
    {
        [Option(Required = true, HelpText = "The folder containing folders to pack in the same format as extracted. i.e. In these folders should be subfolders, each of which is an unique ID.")]
        public string Source { get; internal set; }

        [Option(Required = true, HelpText = "The folder in which to save the new archive files.")]
        public string SavePath { get; internal set; }

        [Option(Required = false, HelpText = "Set to true if file uses Big Endian.", Default = false)]
        public bool BigEndian { get; internal set; }
    }

    [Verb("pack", HelpText = "Packs a Riders Archive file.")]
    internal class PackOptions
    {
        [Option(Required = true, HelpText = "The folder containing the files to pack in the same format as extracted. i.e. In this folder should be subfolders, each of which is an unique ID.")]
        public string Source { get; internal set; }

        [Option(Required = true, HelpText = "The path to which to save the new archive.")]
        public string SavePath { get; internal set; }

        [Option(Required = false, HelpText = "Set to true if file uses Big Endian.", Default = false)]
        public bool BigEndian { get; internal set; }
    }

    [Verb("packlist", HelpText = "Packs a list of Riders Archive files.")]
    internal class PackListOptions
    {
        [Option(Required = true, HelpText = "Text file containing folders with the files to pack in the same format as extracted. Each folder should be on a separate line.")]
        public string Sources { get; private set; }

        [Option(Required = true, HelpText = "Text file containing paths where the new archives should be saved. Each file should be on a separate line.")]
        public string SavePaths { get; private set; }

        [Option(Required = false, HelpText = "Set to true if file uses Big Endian.", Default = false)]
        public bool BigEndian { get; internal set; }
    }

    [Verb("test", HelpText = "Unpacks and repacks all files and compares their hashes.")]
    internal class TestOptions
    {
        [Option(Required = true, HelpText = "Path to the directory containing the files to test.")]
        public string Source { get; private set; }
        
        [Option(Required = false, HelpText = "Set to true if files use Big Endian.", Default = false)]
        public bool BigEndian { get; internal set; }

        [Option(Required = false, HelpText = "Location of the folder to save old and new file in case of mismatch.", Default = false)]
        public string SavePath { get; private set; }
    }
}
