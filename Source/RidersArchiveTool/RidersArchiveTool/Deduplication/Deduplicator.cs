using System;
using System.Collections.Generic;
using System.IO;
using RidersArchiveTool.Deduplication.Structures;
using Sewer56.SonicRiders.Parser.Archive;
using Sewer56.SonicRiders.Parser.Archive.Structs.Managed;
using Sewer56.SonicRiders.Utility;

namespace RidersArchiveTool.Deduplication;

internal class Deduplicator
{
    /// <summary>
    /// Path where the common archive will be output.
    /// </summary>
    public string CommonOutputPath { get; set; }

    /// <summary>
    /// Set to true to compress the output files.
    /// </summary>
    public bool Compress { get; set; }

    /// <summary>
    /// True to use Big Endian (GCN), false otherwise.
    /// </summary>
    public bool BigEndian { get; set; }

    /// <summary>
    /// The list of files to be used as input to the deduplicator.
    /// </summary>
    public string[] InputFiles { get; set; }

    /// <summary>
    /// Options to use for the archive writer.
    /// </summary>
    public ArchiveWriterOptions ArchiveWriterOptions { get; set; }

    /// <summary>
    /// Options to use for the archive compressor.
    /// </summary>
    public ArchiveCompressorOptions ArchiveCompressorOptions { get; set; }

    internal Deduplicator(DeduplicateOptions options)
    {
        CommonOutputPath = Path.Combine(options.Source, options.SaveName);
        BigEndian = options.BigEndian;
        Compress = options.Compress;
        SetInputFilesFromDirectory(options.Source, false);
        SetDefaultWriterCompressorOptions();
    }

    internal Deduplicator(string outputPath, string[] inputFiles, bool bigEndian, bool compress = true)
    {
        CommonOutputPath = outputPath;
        Compress = compress;
        BigEndian = bigEndian;
        InputFiles = inputFiles;
        SetDefaultWriterCompressorOptions();
    }

    /// <summary>
    /// Assigns files to be processed from a given directory.
    /// </summary>
    /// <param name="directory">The directory to assign files to be processed from.</param>
    /// <param name="includeChildren">Includes child directories.</param>
    public void SetInputFilesFromDirectory(string directory, bool includeChildren)
    {
        if (!includeChildren)
            InputFiles = Directory.GetFiles(directory);
        else
            InputFiles = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
    }

    /// <summary>
    /// Sets the default writer and compressor options based on the set <see cref="BigEndian"/> mode.
    /// </summary>
    public void SetDefaultWriterCompressorOptions()
    {
        ArchiveCompressorOptions = BigEndian ? ArchiveCompressorOptions.GameCube : ArchiveCompressorOptions.PC;
        ArchiveWriterOptions = BigEndian ? ArchiveWriterOptions.GameCube : ArchiveWriterOptions.PC;
    }

    /// <summary>
    /// Returns the combined size of all input files, in bytes.
    /// </summary>
    /// <param name="includeOutputFile">Set to true to include the common file to be output.</param>
    /// <returns>Total size of all input files.</returns>
    public long GetInputFilesSize(bool includeOutputFile)
    {
        long size = 0;
        foreach (var input in InputFiles)
            size += GetFileSize(input);

        if (includeOutputFile && File.Exists(CommonOutputPath))
            size += GetFileSize(CommonOutputPath);

        return size;
    }
    
    /// <summary>
    /// Performs the deduplication operation.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void Deduplicate()
    {
        if (InputFiles == null)
            throw new Exception("Input Files (InputFiles) are null.");

        if (String.IsNullOrEmpty(CommonOutputPath))
            throw new Exception("Output path to the common archive is null or empty.");

        // Get all archives.
        var archives             = GetArchives();
        var hashToCollectionDict = new Dictionary<ulong, List<GroupHashCollection>>();
        var hashCollections      = new List<ItemHashCollection>();

        // Generate hashes.
        foreach (var archive in archives)
        {
            var itemHashesCollection = new ItemHashCollection(archive);
            hashCollections.Add(itemHashesCollection);
            foreach (var hash in itemHashesCollection.Hashes)
            {
                if (!hashToCollectionDict.TryGetValue(hash.Hash, out var list))
                {
                    list = new List<GroupHashCollection>();
                    hashToCollectionDict[hash.Hash] = list;
                }

                list.Add(hash);
            }
        }

        // Create new archives.
        var commonWriter   = new ArchiveWriter();
        var hashSet        = new HashSet<GroupHashCollection>();
        byte commonGroupId = 0;

        for (var archiveId = 0; archiveId < archives.Count; archiveId++)
        {
            var archive = archives[archiveId];
            var hashCollection = hashCollections[archiveId];
            var newArchive     = new ArchiveWriter();

            for (var groupId = 0; groupId < archive.Groups.Length; groupId++)
            {
                var group     = archive.Groups[groupId];
                var groupHash = hashCollection.Hashes[groupId];
                if (hashToCollectionDict.TryGetValue(groupHash.Hash, out var items) && items.Count == archives.Count)
                {
                    if (hashSet.Contains(groupHash))
                        continue;

                    // Add to common
                    hashSet.Add(groupHash);
                    commonWriter.AddGroup(commonGroupId++, group);
                }
                else
                {
                    // Re-add to archive
                    newArchive.AddGroup((byte) groupId, group);
                }
            }

            SaveArchiveToPath(newArchive, archive.FilePath, Compress);
        }

        SaveArchiveToPath(commonWriter, CommonOutputPath, Compress);
    }

    private void SaveArchiveToPath(ArchiveWriter writer, string filePath, bool compress)
    {
        // Hack: Add dummy file if archive is empty.
        if (writer.Groups.Count == 0)
        {
            writer.AddGroup(0, ManagedGroup.Create());
            compress = false;
        }

        using var outputStream = new FileStream(filePath, FileMode.Create);
        if (!compress)
        {
            writer.Write(outputStream, ArchiveWriterOptions);
        }
        else
        {
            var memoryStream = new MemoryStream(writer.EstimateFileSize(ArchiveWriterOptions));
            writer.Write(memoryStream, ArchiveWriterOptions);
            memoryStream.Position = 0;
            outputStream.Write(ArchiveCompression.CompressFast(memoryStream, (int)memoryStream.Length, ArchiveCompressorOptions));
        }
    }

    private List<DeduplicatorItem> GetArchives()
    {
        var files    = InputFiles;
        var archives = new List<DeduplicatorItem>();

        foreach (var file in files)
        {
            using var fileStream = new FileStream(file, FileMode.Open);
            byte[] buffer;

            if (!ArchiveCompression.IsCompressed(fileStream, BigEndian))
            {
                buffer = GC.AllocateUninitializedArray<byte>((int)fileStream.Length);
                fileStream.TryReadSafe(buffer);
            }
            else
            {
                buffer = ArchiveCompression.DecompressFast(fileStream, (int)fileStream.Length, ArchiveCompressorOptions);
            }

            using var archiveStream = new MemoryStream(buffer);
            using var archiveReader = new ArchiveReader(archiveStream, (int)archiveStream.Length, BigEndian);
            archives.Add(new DeduplicatorItem()
            {
                FilePath = file,
                Groups = archiveReader.GetAllGroups()
            });
        }

        return archives;
    }

    private static long GetFileSize(string path) => new FileInfo(path).Length;
}