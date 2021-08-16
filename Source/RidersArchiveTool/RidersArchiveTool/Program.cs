using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using CommandLine.Text;
using Reloaded.Memory.Streams;
using Reloaded.Memory.Streams.Readers;
using Reloaded.Memory.Streams.Writers;
using RidersArchiveTool.Utilities;
using Sewer56.SonicRiders.Parser.Archive;
using Sewer56.SonicRiders.Parser.Archive.Structs.Managed;
using Standart.Hash.xxHash;
using File = System.IO.File;

namespace RidersArchiveTool
{
    class Program
    {
        private const char GroupIdSeparator = '_';

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

            var parserResult = parser.ParseArguments<ExtractOptions, PackOptions, PackAllOptions, TestOptions>(args);
            parserResult.WithParsed<ExtractOptions>(Extract)
                        .WithParsed<PackOptions>(Pack)
                        .WithParsed<PackAllOptions>(PackAll)
                        .WithParsed<TestOptions>(TestFiles)
                        .WithNotParsed(errs => HandleParseError(parserResult, errs));
        }

        private static void TestFiles(TestOptions options)
        {
            if (!Directory.Exists(options.SavePath))
                Directory.CreateDirectory(options.SavePath);

            var files = Directory.GetFiles(options.Source);
            var archiveGuesser = new RidersArchiveGuesser();
            var mismatchCount = 0;
            var filesTested = 0;

            foreach (var file in files)
            {
                var data = File.ReadAllBytes(file);
                using var stream = new MemoryStream(data);
                using var bufferedStreamReader = new BufferedStreamReader(stream, 2048);
                using EndianStreamReader endianStreamReader = options.BigEndian ? new BigEndianStreamReader(bufferedStreamReader) : new LittleEndianStreamReader(bufferedStreamReader);

                if (archiveGuesser.TryGuess(endianStreamReader, (int) stream.Length))
                {
                    stream.Position = 0;
                    TestFile(options, stream, Path.GetFileName(file), ref mismatchCount);
                    filesTested++;
                }
                else
                {
                    Console.WriteLine($"[{Path.GetFileName(file)}] Not an Archive File!!");
                }
            }

            if (mismatchCount > 0)
                Console.WriteLine($"Total No. Mismatches {mismatchCount}");

            Console.WriteLine($"Total Files Tested {filesTested}");
        }

        private static void TestFile(TestOptions options, MemoryStream stream, string fileName, ref int mismatchCount)
        {
            var oldFile = stream.ToArray();
            var unpacked = new ArchiveReader(stream, (int)stream.Length, options.BigEndian);
            var writer = new ArchiveWriter();

            // Generate File
            for (var x = 0; x < unpacked.Groups.Length; x++)
            {
                var group = unpacked.Groups[x];
                var newGroup = new ManagedGroup() { Id = group.Id, Files = new List<ManagedFile>() };
                foreach (var file in unpacked.GetFiles(group))
                    newGroup.Files.Add(new ManagedFile() { Data = file });

                writer.AddGroup((byte)x, newGroup);
            }

            // Write File
            using var memoryStream = new MemoryStream();
            writer.Write(memoryStream, options.BigEndian ? ArchiveWriterOptions.GameCube : ArchiveWriterOptions.PC);

            // File Compare
            var newFile = memoryStream.ToArray();
            
            var oldHash = xxHash64.ComputeHash(oldFile);
            var newHash = xxHash64.ComputeHash(newFile);

            if (oldHash != newHash)
            {
                Console.WriteLine($"[{fileName}] Hash Mismatch | Old {oldHash}, New {newHash}");
                mismatchCount++;

                if (!string.IsNullOrEmpty(options.SavePath))
                {
                    var filePathNew = Path.Combine(options.SavePath, $"{fileName}.new");
                    var filePathOld = Path.Combine(options.SavePath, $"{fileName}.old");
                    File.WriteAllBytes(filePathOld, oldFile);
                    File.WriteAllBytes(filePathNew, newFile);
                }
            }

            var oldLength = oldFile.Length;
            var newLength = newFile.Length;
            if (oldLength != newLength)
                Console.WriteLine($"[{fileName}] Length Mismatch | Old {oldLength}, New {newLength}");
        }

        private static void PackAll(PackAllOptions packAllOptions)
        {
            var sources = File.ReadAllLines(packAllOptions.Sources);
            var paths   = File.ReadAllLines(packAllOptions.SavePaths);

            if (sources.Length != paths.Length)
                throw new ArgumentException("Amount of source folders does not equal amount of save paths.");

            for (int x = 0; x < sources.Length; x++)
            {
                Console.WriteLine($"Saving: {paths[x]}");
                Pack(new PackOptions() { SavePath = paths[x], Source = sources[x], BigEndian = packAllOptions.BigEndian });
            }
        }

        private static void Pack(PackOptions options)
        {
            var directories = Directory.GetDirectories(options.Source);
            var writer      = new ArchiveWriter();

            foreach (var dir in directories)
            {
                var directoryName = Path.GetFileNameWithoutExtension(dir).Split(GroupIdSeparator);

                var groupNo = Convert.ToByte(directoryName[0]);
                var id      = Convert.ToUInt16(directoryName[1]);
                var filesInside = Directory.GetFiles(dir);

                var group = new ManagedGroup() { Id = id, Files = new List<ManagedFile>(filesInside.Length) };
                foreach (var file in filesInside)
                    group.Files.Add(new ManagedFile() { Data = File.ReadAllBytes(file) });

                writer.AddGroup(groupNo, group);
            }

            // Write file to new location.
            Directory.CreateDirectory(Path.GetDirectoryName(options.SavePath));
            using var fileStream = new FileStream(options.SavePath, FileMode.Create, FileAccess.Write, FileShare.None);
            writer.Write(fileStream, options.BigEndian ? ArchiveWriterOptions.GameCube : ArchiveWriterOptions.PC);
        }

        private static void Extract(ExtractOptions options)
        {
            Directory.CreateDirectory(options.SavePath);

            using var fileStream   = new FileStream(options.Source, FileMode.Open, FileAccess.Read);
            using var archiveReader = new ArchiveReader(fileStream, (int) fileStream.Length, options.BigEndian);
            var allGroups = archiveReader.GetAllGroups();

            for (var x = 0; x < allGroups.Length; x++)
            {
                var group  = allGroups[x];
                var folder = Path.Combine(options.SavePath, $"{x:000}{GroupIdSeparator}{group.Id:00000}");
                Directory.CreateDirectory(folder);

                for (var y = 0; y < group.Files.Count; y++)
                {
                    var filePath = Path.Combine(folder, y.ToString("00000"));
                    File.WriteAllBytes(filePath, group.Files[y].Data);
                    Console.WriteLine($"Writing {filePath}");
                }
            }
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
