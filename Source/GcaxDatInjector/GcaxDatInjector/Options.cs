using CommandLine;

namespace GcaxDatInjector;

[Verb("extract", HelpText = "Extracts sounds from a DAT archive.")]
internal class ExtractOptions
{
    [Option(Required = true, HelpText = "The .DAT file to extract sounds from.")]
    public string Source { get; internal set; } = "";

    [Option(Required = true, HelpText = "Where to save the extracted sounds.")]
    public string Destination { get; internal set; } = "";

    [Option(Required = false, HelpText = "Whether the audio should be converted to WAV.", Default = false)]
    public bool? Convert { get; internal set; }
}

[Verb("inject", HelpText = "Injects sounds into DAT archive.")]
internal class InjectOptions
{
    [Option(Required = true, HelpText = "The folder with the sounds to be injected.")]
    public string Source { get; internal set; } = "";

    [Option(Required = true, HelpText = "The DAT file to save the new sounds in.")]
    public string Destination { get; internal set; } = "";

    [Option(Required = false, HelpText = "Prints a gecko code for Sonic Riders [NTSC-U] required to run the file.", Default = true)]
    public bool PrintGecko { get; internal set; }
}