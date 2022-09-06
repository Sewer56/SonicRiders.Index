// See https://aka.ms/new-console-template for more information
using CommandLine;
using CommandLine.Text;
using GcaxDatInjector;

var parser = new Parser(with =>
{
    with.AutoHelp = true;
    with.CaseSensitive = false;
    with.CaseInsensitiveEnumValues = true;
    with.EnableDashDash = true;
    with.HelpWriter = null;
});

var parserResult = parser.ParseArguments<ExtractOptions, InjectOptions>(args);
parserResult.WithParsed<ExtractOptions>(Injector.Extract)
    .WithParsed<InjectOptions>(Injector.Inject)
    .WithNotParsed(errs => HandleParseError(parserResult, errs));

static void HandleParseError(ParserResult<object> options, IEnumerable<Error> errs)
{
    var helpText = HelpText.AutoBuild(options, help =>
    {
        help.Copyright = "Created by Sewer56, licensed under MIT License";
        help.AutoHelp = false;
        help.AutoVersion = false;
        help.AddDashesToOption = true;
        help.AddEnumValuesToHelpText = true;
        help.AdditionalNewLineAfterOption = true;
        return HelpText.DefaultParsingErrorsHandler(options, help);
    }, example => example, true);

    Console.WriteLine(helpText);
}