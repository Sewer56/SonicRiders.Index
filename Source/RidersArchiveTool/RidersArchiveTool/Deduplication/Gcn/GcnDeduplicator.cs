using System;
using System.Collections.Generic;
using System.IO;
using Sewer56.SonicRiders.Structures.Enums;

namespace RidersArchiveTool.Deduplication.Gcn;

internal class GcnDeduplicator
{
    public static void Deduplicate(DeduplicateGcnOptions options)
    {
        // Deduplicating Stages
        DeduplicateStages(options);
    }

    private static void DeduplicateStages(DeduplicateGcnOptions options)
    {
        Console.WriteLine($"=== Stage Deduplication Begin ===");
        const string Common1P = "CMN1";
        const string Common2P = "CMN2";
        const string Common4P = "CMN4";
        const string CommonMission = "MCMN";
        const string Common = "CMN";
        
        var targetFolder  = CombineAndCreateDirectory(options.SavePath, "Stages");

        // Copy Stage Files & Populate
        var missionFiles = new List<string>();
        var stageFiles1P = new List<string>();
        var stageFiles2P = new List<string>();
        var stageFiles4P = new List<string>();

        for (int x = 0; x <= (int)Levels.SpaceTheater; x++)
        {
            CopyFile(options.Source, targetFolder, $"{x}", $"{x}", stageFiles1P);
            CopyFile(options.Source, targetFolder, $"{x}", $"{x}M", stageFiles2P);
            CopyFile(options.Source, targetFolder, $"{x}", $"{x}V", stageFiles4P);
            CopyFile(options.Source, targetFolder, $"{x}", $"M{x}", missionFiles);
        }

        var originalDataSize = GetDirectorySize(targetFolder);

        // Deduplicate 1P Stages
        Deduplicate(Path.Combine(targetFolder, Common1P), stageFiles1P.ToArray(), out var originalSize1P, out var newSize1P);
        LogDeduplication("1P Stage Assets", originalSize1P, newSize1P);

        // Deduplicate 2P Stages
        Deduplicate(Path.Combine(targetFolder, Common2P), stageFiles2P.ToArray(), out var originalSize2P, out var newSize2P);
        LogDeduplication("2P Stage Assets", originalSize2P, newSize2P);

        // Deduplicate 4P Stages
        Deduplicate(Path.Combine(targetFolder, Common4P), stageFiles4P.ToArray(), out var originalSize4P, out var newSize4P);
        LogDeduplication("4P Stage Assets", originalSize4P, newSize4P);

        // Deduplicate Mission Stages
        Deduplicate(Path.Combine(targetFolder, CommonMission), missionFiles.ToArray(), out var originalSizeMission, out var newSizeMission);
        LogDeduplication("Mission Assets", originalSizeMission, newSizeMission);

        // Deduplicate Commons
        Deduplicate(Path.Combine(targetFolder, Common), new string[]
        {
            Path.Combine(targetFolder, Common1P),
            Path.Combine(targetFolder, Common2P),
            Path.Combine(targetFolder, Common4P)
        }, out var originalSizeCommon, out var newSizeCommon);
        LogDeduplication("1P, 2P, 4P Common", originalSizeCommon, newSizeCommon);

        // Deduplicate Stages with Themselves
        var singleStageFiles = new List<string>();
        for (int x = 0; x <= (int)Levels.SpaceTheater; x++)
        {
            AddFileIfExists(singleStageFiles, CopyFileCombinePath(targetFolder, $"{x}", $"{x}"));
            AddFileIfExists(singleStageFiles, CopyFileCombinePath(targetFolder, $"{x}", $"{x}M"));
            AddFileIfExists(singleStageFiles, CopyFileCombinePath(targetFolder, $"{x}", $"{x}V"));
            AddFileIfExists(singleStageFiles, CopyFileCombinePath(targetFolder, $"{x}", $"M{x}"));

            Deduplicate(CopyFileCombinePath(targetFolder, $"{x}", $"{x}CMN"), singleStageFiles.ToArray(), out var originalStageSize, out var newStageSize);
            LogDeduplication($"Stage {x} Common", originalStageSize, newStageSize);
            singleStageFiles.Clear();
        }

        // Print Space Savings
        var newDataSize = GetDirectorySize(targetFolder);
        Console.WriteLine($"=== Stage Deduplication Complete ===\n" +
                          $"==> Space Saved Total: {((originalDataSize - newDataSize) / 1000.0):##.00}KBytes");
    }

    static void Deduplicate(string outputPath, string[] inputFiles, out long originalSize, out long newSize)
    {
        var deduplicator = new Deduplicator(outputPath, inputFiles, true, true);
        originalSize = deduplicator.GetInputFilesSize(false);
        deduplicator.Deduplicate();
        newSize = deduplicator.GetInputFilesSize(true);
    }

    static void LogDeduplication(string title, long originalSize, long newSize)
    {
        Console.WriteLine($"[{title}] Deduplication Statistics\n" +
                          $"Old Size: {originalSize}\n" +
                          $"New Size: {newSize}\n" +
                          $"Space Saved: {((originalSize - newSize) / 1000.0):##.00}KBytes");
    }

    static string CombineAndCreateDirectory(params string[] paths)
    {
        var targetFolder = Path.Combine(paths);
        Directory.CreateDirectory(targetFolder);
        return targetFolder;
    }

    static void AddFileIfExists(List<string> files, string filePath)
    {
        if (File.Exists(filePath))
            files.Add(filePath);
    }

    /// <summary>
    /// Path combine, same way as <see cref="CopyFile"/> does.
    /// </summary>
    static string CopyFileCombinePath(string targetFolder, string targetSubfolderName, string fileName)
    {
        return Path.Combine(targetFolder, targetSubfolderName, fileName);
    }

    /// <summary/>
    /// <param name="sourceFolder">Source folder containing the file.</param>
    /// <param name="targetFolder">Target folder to receive the file.</param>
    /// <param name="targetSubfolderName">Any additional subfolders, if required.</param>
    /// <param name="fileName">File name in source folder.</param>
    /// <param name="files">Optional list of files to add the new file path to.</param>
    static void CopyFile(string sourceFolder, string targetFolder, string targetSubfolderName, string fileName, List<string> files = null)
    {
        var filePath = Path.Combine(sourceFolder, fileName);
        if (!File.Exists(filePath))
            return;

        var directory = Path.Combine(targetFolder, targetSubfolderName);
        var newPath   = Path.Combine(directory, fileName);
        Directory.CreateDirectory(directory);
        File.Copy(filePath, newPath, true);

        files?.Add(newPath);
    }

    /// <summary>
    /// Gets the size of a directory and all of its children.
    /// </summary>
    /// <param name="path">Path to the directory.</param>
    /// <returns>Combined size of all files in directory.</returns>
    static long GetDirectorySize(string path)
    {
        long bytes = 0;
        foreach (string name in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            bytes += new FileInfo(name).Length;

        return bytes;
    }
}