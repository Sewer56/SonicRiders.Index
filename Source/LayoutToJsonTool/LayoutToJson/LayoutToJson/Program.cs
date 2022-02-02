using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommandLine;
using CommandLine.Text;
using Reloaded.Memory.Streams;
using Reloaded.Memory.Streams.Readers;
using Reloaded.Memory.Streams.Writers;
using Sewer56.SonicRiders.Parser.Menu.Metadata.Managed;

namespace LayoutToJson
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser(with =>
            {
                with.AutoHelp = true;
                with.CaseSensitive = false;
                with.CaseInsensitiveEnumValues = true;
                with.EnableDashDash = true;
                with.HelpWriter = null;
            });

            var parserResult = parser.ParseArguments<ToJsonOptions, FromJsonOptions>(args);
            parserResult.WithParsed<ToJsonOptions>(ToJson)
                .WithParsed<FromJsonOptions>(FromJson)
                .WithNotParsed(errs => HandleParseError(parserResult, errs));
        }

        private static void FromJson(FromJsonOptions options)
        {
            var source = JsonSerializer.Deserialize<ManagedMenuMetadata>(File.ReadAllText(options.Source), ManagedMenuMetadata.SerializerOptions);

            using var extendedMemoryStream  = new ExtendedMemoryStream(1024 * 512);
            using EndianMemoryStream writer = options.BigEndian ? new BigEndianMemoryStream(extendedMemoryStream) : new LittleEndianMemoryStream(extendedMemoryStream);
            source.ToStream(writer);

            using FileStream fileOut = new FileStream(options.Destination, FileMode.Create);
            writer.Stream.Seek(0, SeekOrigin.Begin);
            writer.Stream.CopyTo(fileOut);
        }

        private static void ToJson(ToJsonOptions options)
        {
            using var fileStream = new FileStream(options.Source, FileMode.Open);
            using var bufferedStreamReader = new BufferedStreamReader(fileStream, 2048);
            using EndianStreamReader reader = options.BigEndian ? new BigEndianStreamReader(bufferedStreamReader) : new LittleEndianStreamReader(bufferedStreamReader);
            var metadata = ManagedMenuMetadata.FromStream(reader);
            var json = JsonSerializer.Serialize(metadata, ManagedMenuMetadata.SerializerOptions);

            File.WriteAllText(options.Destination, json);
        }

        /// <summary>
        /// Errors or --help or --version.
        /// </summary>
        static void HandleParseError(ParserResult<object> options, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(options, help =>
            {
                help.Copyright = "Created by Sewer56, licensed under GNU LGPL V3";
                help.AutoHelp = false;
                help.AutoVersion = false;
                help.AddDashesToOption = true;
                help.AddEnumValuesToHelpText = true;
                help.AdditionalNewLineAfterOption = true;
                return HelpText.DefaultParsingErrorsHandler(options, help);
            }, example => example, true);

            Console.WriteLine(helpText);
        }
    }
}
