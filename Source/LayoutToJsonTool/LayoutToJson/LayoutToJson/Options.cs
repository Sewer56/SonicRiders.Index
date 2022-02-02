using CommandLine;

namespace LayoutToJson
{
    [Verb("tojson", HelpText = "Converts an existing layout file to JSON.")]
    internal class ToJsonOptions
    {
        [Option(Required = true, HelpText = "The file to convert.")]
        public string Source { get; internal set; }

        [Option(Required = true, HelpText = "Where to save the new file.")]
        public string Destination { get; internal set; }

        [Option(Required = false, HelpText = "Set to true if file uses Big Endian (GameCube), else false for PC and others.", Default = false)]
        public bool BigEndian { get; internal set; }
    }

    [Verb("fromjson", HelpText = "Converts a JSON file into a layout file.")]
    internal class FromJsonOptions
    {
        [Option(Required = true, HelpText = "The file to convert.")]
        public string Source { get; internal set; }

        [Option(Required = true, HelpText = "Where to save the new file.")]
        public string Destination { get; internal set; }

        [Option(Required = false, HelpText = "Set to true if file uses Big Endian (GameCube), else false for PC and others.", Default = false)]
        public bool BigEndian { get; internal set; }
    }
}
