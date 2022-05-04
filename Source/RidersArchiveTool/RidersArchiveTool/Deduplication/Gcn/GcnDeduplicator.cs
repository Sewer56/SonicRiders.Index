using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sewer56.SonicRiders.Structures.Enums;

namespace RidersArchiveTool.Deduplication.Gcn;

internal class GcnDeduplicator
{
    private static DeduplicateGcnOptions _options;

    public static void Deduplicate(DeduplicateGcnOptions options)
    {
        _options = options;

        // Deduplicating Stages
        DeduplicateStages();
        DeduplicateTag();
        // SVR: NO
        DeduplicateCutscenes();
        ApplyFiles();
    }

    private static void ApplyFiles()
    {
        if (!_options.Apply)
            return;

        if (string.IsNullOrEmpty(_options.ApplyPath))
            _options.ApplyPath = _options.SavePath;

        var files = Directory.GetFiles(_options.SavePath, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var targetPath = Path.Combine(_options.ApplyPath, Path.GetFileName(file));
            File.Move(file, targetPath, true);
        }
    }

    private class CutsceneData
    {
        public List<string> MainCutsceneFiles = new();
        public List<List<string>> Scenes = new();
    }

    private static void DeduplicateCutscenes()
    {
        Console.WriteLine($"=== Cutscene Deduplication Begin ===");
        const string Common = "SCMN";
        const char CommonLanguageSuffix = 'C';

        var languageSuffixes = new char[] { 'E', 'F', 'G', 'I', 'J', 'S' };
        var targetFolder     = CombineAndCreateDirectory(_options.SavePath, "Cutscene");

        // Copy All Cutscene Files.
        var cutscenes = new List<CutsceneData>();
        for (int cutsceneId = 0; cutsceneId <= 16; cutsceneId++)
        {
            var cutscene = new CutsceneData();

            // Main Files
            foreach (var suffix in languageSuffixes)
                CopyFile(_options.Source, targetFolder, $"{cutsceneId}", $"S{cutsceneId}{suffix}", cutscene.MainCutsceneFiles);

            // Scenes
            for (int sceneId = 0; sceneId < 99; sceneId++)
            {
                var baseName = $"S{cutsceneId}S{sceneId}";
                var sceneFiles = new List<string>();

                foreach (var suffix in languageSuffixes)
                {
                    var fileName = baseName + suffix;
                    CopyFile(_options.Source, targetFolder, $"{cutsceneId}/Scene{sceneId}", fileName, sceneFiles);
                }

                if (sceneFiles.Count > 0)
                    cutscene.Scenes.Add(sceneFiles);
            }

            // Add if valid cutscene
            if (cutscene.MainCutsceneFiles.Count > 0)
                cutscenes.Add(cutscene);
        }

        // Calc Data Size
        var originalDataSize = GetDirectorySize(targetFolder);

        // Cutscene Files: Deduplicate
        var commonCutsceneFiles      = new List<string>();

        for (var x = 0; x < cutscenes.Count; x++)
        {
            var cutscene = cutscenes[x];

            // Deduplicate Main
            if (cutscene.MainCutsceneFiles.Count >= 2)
            {
                var commonCutsceneFile = ReplaceLastCharacter(cutscene.MainCutsceneFiles.First(), CommonLanguageSuffix);
                commonCutsceneFiles.Add(commonCutsceneFile);
                Deduplicate(commonCutsceneFile, cutscene.MainCutsceneFiles.ToArray(), out var origMainCutSize, out var newMainCutSize);
                LogDeduplication($"Cutscene {x}", origMainCutSize, newMainCutSize);
            }

            // Deduplicate Scene
            if (cutscene.Scenes.Count > 0)
            {
                for (var y = 0; y < cutscene.Scenes.Count; y++)
                {
                    var sceneFiles = cutscene.Scenes[y];
                    if (sceneFiles.Count >= 2)
                    {
                        var commonSceneFile = ReplaceLastCharacter(sceneFiles.First(), CommonLanguageSuffix);
                        Deduplicate(commonSceneFile, sceneFiles.ToArray(), out var origMainCutSize, out var newMainCutSize);
                        LogDeduplication($"Cutscene {x} Scene #{y}", origMainCutSize, newMainCutSize);
                    }
                }
            }
        }

        // Main Common Files: Deduplicate
        Deduplicate(Path.Combine(targetFolder, Common), commonCutsceneFiles.ToArray(), out var origCommonSize, out var newCommonSize);
        LogDeduplication($"Cutscene Common", origCommonSize, newCommonSize);

        // Print Space Savings
        var newDataSize = GetDirectorySize(targetFolder);
        Console.WriteLine($"=== Cutscene Deduplication Complete ===\n" +
                          $"==> Space Saved Total: {((originalDataSize - newDataSize) / 1000.0):##.00}KBytes");
    }

    private static void DeduplicateTag()
    {
        Console.WriteLine($"=== Tag Deduplication Begin ===");
        const string Common = "TCMN";

        var targetFolder = CombineAndCreateDirectory(_options.SavePath, "Tag");

        // Copy Stage Files & Populate
        var tagFiles = new List<string>();
        for (int x = 0; x <= (int)Levels.SpaceTheater; x++)
            CopyFile(_options.Source, targetFolder, $"", $"TG{x}", tagFiles);

        var originalDataSize = GetDirectorySize(targetFolder);

        // Deduplicate Tag
        Deduplicate(Path.Combine(targetFolder, Common), tagFiles.ToArray(), out var originalSize, out var newSize);
        LogDeduplication("Tag Assets", originalSize, newSize);
        
        // Print Space Savings
        var newDataSize = GetDirectorySize(targetFolder);
        Console.WriteLine($"=== Tag Deduplication Complete ===\n" +
                          $"==> Space Saved Total: {((originalDataSize - newDataSize) / 1000.0):##.00}KBytes");
    }

    private static void DeduplicateStages()
    {
        Console.WriteLine($"=== Stage Deduplication Begin ===");
        const string Common1P = "CMN1";
        const string Common2P = "CMN2";
        const string Common4P = "CMN4";
        const string CommonMission = "MCMN";
        const string Common = "CMN";
        
        var targetFolder  = CombineAndCreateDirectory(_options.SavePath, "Stages");

        // Copy Stage Files & Populate
        var missionFiles = new List<string>();
        var stageFiles1P = new List<string>();
        var stageFiles2P = new List<string>();
        var stageFiles4P = new List<string>();

        for (int x = 0; x <= (int)Levels.SpaceTheater; x++)
        {
            CopyFile(_options.Source, targetFolder, $"{x}", $"{x}", stageFiles1P);
            CopyFile(_options.Source, targetFolder, $"{x}", $"{x}M", stageFiles2P);
            CopyFile(_options.Source, targetFolder, $"{x}", $"{x}V", stageFiles4P);
            CopyFile(_options.Source, targetFolder, $"{x}", $"M{x}", missionFiles);
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
            Path.Combine(targetFolder, Common4P),
            Path.Combine(targetFolder, CommonMission)
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
        var deduplicator = new Deduplicator(outputPath, inputFiles, true, _options.Compress);
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
    /// <returns>Path to the new file if copied, else null.</returns>
    static string CopyFile(string sourceFolder, string targetFolder, string targetSubfolderName, string fileName, List<string> files = null)
    {
        var filePath = Path.Combine(sourceFolder, fileName);
        if (!File.Exists(filePath))
            return null;

        var directory = Path.Combine(targetFolder, targetSubfolderName);
        var newPath   = Path.Combine(directory, fileName);
        Directory.CreateDirectory(directory);
        File.Copy(filePath, newPath, true);

        files?.Add(newPath);
        return newPath;
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

    /// <summary>
    /// Replaces last character of a string.
    /// </summary>
    /// <param name="text">The text to replace the character in.</param>
    /// <param name="newChar">The character to insert.</param>
    static string ReplaceLastCharacter(string text, char newChar) => text.Remove(text.Length - 1) + newChar;
}