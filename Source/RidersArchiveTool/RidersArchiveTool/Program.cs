using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

            var parserResult = parser.ParseArguments<ExtractOptions, ExtractAllOptions, PackOptions, PackAllOptions, PackListOptions, TestOptions, CompressOptions, DecompressOptions, FindDecompressedOptions>(args);
            parserResult.WithParsed<ExtractOptions>(Extract)
                        .WithParsed<ExtractAllOptions>(ExtractAll)
                        .WithParsed<PackOptions>(Pack)
                        .WithParsed<PackAllOptions>(PackAll)
                        .WithParsed<PackListOptions>(PackList)
                        .WithParsed<TestOptions>(TestFiles)
                        .WithParsed<CompressOptions>(CompressFile)
                        .WithParsed<DecompressOptions>(DecompressFile)
                        .WithParsed<FindDecompressedOptions>(FindDecompressed)
                        .WithNotParsed(errs => HandleParseError(parserResult, errs));
        }

        private static void FindDecompressed(FindDecompressedOptions options)
        {
            var watch = Stopwatch.StartNew();
            var files = Directory.GetFiles(options.Source, "*.*", SearchOption.AllDirectories);
            var archivesTested = 0;

            foreach (var file in files)
            {
                using var stream = new FileStream(file, FileMode.Open);

                // Check if Valid Riders Archive
                if (!RidersArchiveGuesser.TryGuess(stream, (int)stream.Length, options.BigEndian, out bool isCompressed))
                    continue;

                if (!isCompressed)
                {
                    if (options.PrintCompressedSavings)
                    {
                        var compressed = ArchiveCompression.CompressFast(stream, (int)stream.Length, options.BigEndian ? ArchiveCompressorOptions.GameCube : ArchiveCompressorOptions.PC);
                        Console.WriteLine($"[Uncompressed] {file}, Before: {stream.Length}, After: {compressed.Length}, Ratio: {((float)compressed.Length / stream.Length):0.00}");
                    }
                    else
                    {
                        Console.WriteLine($"[Uncompressed] {file}");
                    }
                }

                archivesTested++;
            }

            Console.WriteLine($"Total Archives Checked {archivesTested}");
            Console.WriteLine($"{watch.ElapsedMilliseconds}ms");
        }

        private static void TestFiles(TestOptions options)
        {
            var watch = Stopwatch.StartNew();
            if (!Directory.Exists(options.SavePath))
                Directory.CreateDirectory(options.SavePath);

            var files = Directory.GetFiles(options.Source);
            var mismatchCount = 0;
            var archivesTested = 0;

            foreach (var file in files)
            {
                using var stream = new MemoryStream(File.ReadAllBytes(file));

                if (!RidersArchiveGuesser.TryGuess(stream, (int)stream.Length, options.BigEndian, out bool isCompressed))
                {
                    Console.WriteLine($"[{Path.GetFileName(file)}] Not an Archive File!!");
                    continue;
                }

                if (isCompressed)
                {
                    using var decompStream = new MemoryStream(ArchiveCompression.DecompressSlow(stream, options.BigEndian ? ArchiveCompressorOptions.GameCube : ArchiveCompressorOptions.PC));
                    TestFile(options, decompStream, Path.GetFileName(file), ref mismatchCount);
                }
                else
                {
                    TestFile(options, stream, Path.GetFileName(file), ref mismatchCount);
                }

                archivesTested++;
            }

            if (mismatchCount > 0)
                Console.WriteLine($"Total No. Mismatches {mismatchCount}");

            Console.WriteLine($"Total Archives Tested {archivesTested}");
            Console.WriteLine($"{watch.ElapsedMilliseconds}ms");
        }

        private static void TestFile(TestOptions options, MemoryStream fileData, string fileName, ref int mismatchCount)
        {
            var oldFile = fileData.ToArray();
            var unpacked = new ArchiveReader(fileData, (int)fileData.Length, options.BigEndian);
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

        private static void PackList(PackListOptions packAllOptions)
        {
            var sources = File.ReadAllLines(packAllOptions.Sources);
            var paths   = File.ReadAllLines(packAllOptions.SavePaths);

            if (sources.Length != paths.Length)
                throw new ArgumentException("Amount of source folders does not equal amount of save paths.");

            var partitioner = Partitioner.Create(0, sources.Length);
            Parallel.ForEach(partitioner, (tuple, state) =>
            {
                for (int x = tuple.Item1; x < tuple.Item2; x++)
                    Pack(new PackOptions() { SavePath = paths[x], Source = sources[x], BigEndian = packAllOptions.BigEndian, Compress = packAllOptions.Compress });
            });
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
            if (!options.Compress)
            {
                writer.Write(fileStream, options.BigEndian ? ArchiveWriterOptions.GameCube : ArchiveWriterOptions.PC);
            }
            else
            {
                // TODO: Optimize this more. This is pretty unoptimal.
                var writerOptions      = options.BigEndian ? ArchiveWriterOptions.GameCube : ArchiveWriterOptions.PC;
                using var memoryStream = new MemoryStream(writer.EstimateFileSize(writerOptions) + 1);
                writer.Write(memoryStream, writerOptions);
                memoryStream.Position = 0;
                fileStream.Write(ArchiveCompression.CompressFast(memoryStream, (int)memoryStream.Length, options.BigEndian ? ArchiveCompressorOptions.GameCube : ArchiveCompressorOptions.PC));
            }

            Console.WriteLine($"Saved: {options.SavePath}");
        }

        private static void PackAll(PackAllOptions options)
        {
            var directories = Directory.GetDirectories(options.Source);
            Directory.CreateDirectory(options.SavePath);

            var partitioner = Partitioner.Create(0, directories.Length);
            Parallel.ForEach(partitioner, tuple =>
            {
                for (int x = tuple.Item1; x < tuple.Item2; x++)
                    PackAllDirectory(options, directories[x]);
            });
        }

        private static void PackAllDirectory(PackAllOptions options, string directory)
        {
            Pack(new PackOptions()
            {
                BigEndian = options.BigEndian,
                SavePath = Path.Combine(options.SavePath, Path.GetFileNameWithoutExtension(directory)),
                Source = directory
            });
        }

        private static void ExtractAll(ExtractAllOptions options)
        {
            var files = Directory.GetFiles(options.Source);
            Directory.CreateDirectory(options.SavePath);

            var partitioner = Partitioner.Create(0, files.Length);
            Parallel.ForEach(partitioner, tuple =>
            {
                for (int x = tuple.Item1; x < tuple.Item2; x++)
                    ExtractAllFile(options, files[x]);
            });
        }

        private static void ExtractAllFile(ExtractAllOptions options, string filePath)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            if (RidersArchiveGuesser.TryGuess(fileStream, (int)fileStream.Length, options.BigEndian, out bool isCompressed))
                ExtractUsingStream(new ExtractOptions()
                {
                    Source = filePath,
                    SavePath = Path.Combine(options.SavePath, Path.GetFileName(filePath)),
                    BigEndian = options.BigEndian,
                    Silent = true
                }, fileStream, isCompressed);
            else
                Console.WriteLine($"[{Path.GetFileName(filePath)}] Not an Archive File!!");
        }

        private static void Extract(ExtractOptions options) => ExtractUsingStream(options, new FileStream(options.Source, FileMode.Open, FileAccess.Read));
        private static void ExtractUsingStream(ExtractOptions options, FileStream fileStream, bool? isCompressed = null)
        {
            Directory.CreateDirectory(options.SavePath);
            isCompressed ??= ArchiveCompression.IsCompressed(fileStream, options.BigEndian);

            if (isCompressed.Value)
            {
                using var decompressed = new MemoryStream(ArchiveCompression.DecompressFast(fileStream, (int)fileStream.Length, options.BigEndian ? ArchiveCompressorOptions.GameCube : ArchiveCompressorOptions.PC));
                ExtractInternal(options, decompressed);
            }
            else
            {
                ExtractInternal(options, fileStream);
            }
        }

        private static void ExtractInternal(ExtractOptions options, Stream stream)
        {
            using var archiveReader = new ArchiveReader(stream, (int)stream.Length, options.BigEndian);
            var allGroups = archiveReader.GetAllGroups();

            for (var x = 0; x < allGroups.Length; x++)
            {
                var group = allGroups[x];
                var folder = Path.Combine(options.SavePath, $"{x:000}{GroupIdSeparator}{@group.Id:00000}");
                Directory.CreateDirectory(folder);

                for (var y = 0; y < @group.Files.Count; y++)
                {
                    var filePath = Path.Combine(folder, y.ToString("00000"));
                    File.WriteAllBytesAsync(filePath, @group.Files[y].Data);
                    if (!options.Silent)
                        Console.WriteLine($"Written {filePath}");
                }
            }
        }

        private static void DecompressFile(DecompressOptions options)
        {
            using var outputStream = new FileStream(options.Destination, FileMode.Create);
            using var inputStream = new FileStream(options.Source, FileMode.Open);
            outputStream.Write(ArchiveCompression.DecompressFast(inputStream, (int)inputStream.Length, options.BigEndian ? ArchiveCompressorOptions.GameCube : ArchiveCompressorOptions.PC));
        }

        private static void CompressFile(CompressOptions options)
        {
            using var outputStream = new FileStream(options.Destination, FileMode.Create);
            using var inputStream = new FileStream(options.Source, FileMode.Open);
            outputStream.Write(ArchiveCompression.CompressFast(inputStream, (int) inputStream.Length, options.BigEndian ? ArchiveCompressorOptions.GameCube : ArchiveCompressorOptions.PC));
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
