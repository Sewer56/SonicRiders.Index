using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using IndexTool.Misc;
using Sewer56.SonicRiders.Structures.Enums;

namespace IndexTool.Services;

/// <summary>
/// Tries to generate a description given a file name.
/// </summary>
public static class FileDescriptionService
{
    public static Dictionary<string, string> NameToDescription;
    private static List<(char fileName, string desc)> _animationDescriptionPairs = new List<(char fileName, string desc)>()
    {
        new ('S', "Skate"),
        new ('B', "Bike"),
        new ('O', "Board"),
        new ('E', "Eggman (Exclusive Animations)"),
        new ('P', "Opa Opa (Exclusive Animations)"),
    };

    private static List<(char fileName, string desc)> _languageNamePairs = new List<(char fileName, string desc)>()
    {
        new ('E', "English"),
        new ('F', "French"),
        new ('G', "German"),
        new ('I', "Italian"),
        new ('J', "Japanese"),
        new ('S', "Spanish"),
    };

    private static List<(string fileName, string desc)> _soundEffectNames = new List<(string fileName, string desc)>()
    {
        new ("AIAI", "AiAi"),
        new ("amy", "Amy"),
        new ("cream", "Cream"),
        new ("eggm", "Eggman"),
        new ("er1", "E10000-G"),
        new ("er2", "E10000-R"),
        new ("JET", "Jet"),
        new ("KNUCK", "Knuckles"),
        new ("ROUGE", "Rouge"),
        new ("SHADO", "Shadow"),
        new ("SONIC", "Sonic"),
        new ("SS", "Super Sonic"),
        new ("STORM", "Storm"),
        new ("TAILS", "Tails"),
        new ("ULALA", "Ulala"),
        new ("WAVE", "Wave"),
    };

    static FileDescriptionService()
    {
        NameToDescription = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Generate Stage Stuff
        for (int x = 0; x <= (int) Levels.SpaceTheater; x++)
        {
            var name = ((Levels) x).GetAttributeOfType<DescriptionAttribute>();

            NameToDescription.Add($"{x}", $"Stage: {name.Description} (Single Player).");
            NameToDescription.Add($"{x}TE", $"Stage Intro Text Texture: {name.Description}.");
            NameToDescription.Add($"{x}TJ", $"Stage Intro Text Texture [Japanese]: {name.Description}.");

            NameToDescription.Add($"{x}M", $"Stage: {name.Description} (3-4 Player).");
            NameToDescription.Add($"{x}V", $"Stage: {name.Description} (2 Player).");

            NameToDescription.Add($"M{x}", $"Mission Mode Assets for {name.Description}.");
            NameToDescription.Add($"SVR{x}", $"Survival Race Assets for {name.Description}.");
            NameToDescription.Add($"S{x}.ADX", $"Stage Music: {name.Description}.");
        }

        // Stage Event Audio
        for (int x = 1; x <= 16; x++)
        {
            var stageName = ((Levels)x).GetAttributeOfType<DescriptionAttribute>();
            NameToDescription.Add($"E{x:00}E.ADX", $"Story Event Audio: Event No. {x} (Stage: {stageName.Description}).");
            NameToDescription.Add($"E{x:00}J.ADX", $"Story Event Audio: Event No. {x} (Stage: {stageName.Description}) [Japanese].");
        }

        // Generate Character Animations & Gears
        var gears      = Enum.GetValues<ExtremeGearModel>();
        var characters = Enum.GetValues<Characters>();
            
        foreach (var character in characters)
        {
            var chr = character.GetFilenameChar();
            foreach (var gear in gears)
            {
                string fileName = $"P{chr}{(int)gear:00}";
                NameToDescription.Add(fileName, $"Character [Single Player]: {character} + {gear}");
                NameToDescription.Add(fileName + "M", $"Character [Multiplayer & CPU]: {character} + {gear}");
            }

            foreach (var animPair in _animationDescriptionPairs)
            {
                string fileName = $"P{chr}{animPair.fileName}";
                NameToDescription.Add(fileName, $"Character Animations: {character}, {animPair.desc}");
            }
        }

        // Generate Survival Battle and Race, Tag
        NameToDescription.Add($"SVB", $"Battle Mode Assets");
        NameToDescription.Add($"SVR", $"Survival Race Assets");
        foreach (var langNamePair in _languageNamePairs)
        {
            NameToDescription.Add($"SVB{langNamePair.fileName}", $"Battle Mode Localized Assets: {langNamePair.desc}");
            NameToDescription.Add($"SVR{langNamePair.fileName}", $"Survival Race Localized Assets: {langNamePair.desc}");
        }


        for (int x = 0; x <= (int)Levels.SpaceTheater; x++)
        {
            var stageName = ((Levels)x).GetAttributeOfType<DescriptionAttribute>();
            NameToDescription.Add($"TG{x}", $"Tag Mode Assets: {stageName.Description}");
        }

        foreach (var langNamePair in _languageNamePairs)
            NameToDescription.Add($"TG2D{langNamePair.fileName}", $"Tag Mode 2D Assets: {langNamePair.desc}");

        // Sound Effects
        NameToDescription.Add($"00.dat", $"Sound Effects Archive: Menu");
        NameToDescription.Add($"01.dat", $"Sound Effects Archive: Race");

        foreach (var soundFxPair in _soundEffectNames)
        {
            NameToDescription.Add($"10{soundFxPair.fileName}.dat", $"Sound Effects: {soundFxPair.desc}");
            NameToDescription.Add($"10J{soundFxPair.fileName}.dat", $"Sound Effects: {soundFxPair.desc} (Japanese)");
        }

        for (int x = 1; x <= 8; x++)
        {
            var stageName = ((Levels)x).GetAttributeOfType<DescriptionAttribute>();
            NameToDescription.Add($"05{x:00}.DAT", $"Sound Effects: {stageName.Description}");
        }

        NameToDescription.Add($"0509.DAT", $"Sound Effects: Battle Mode");

        // Menu
        NameToDescription.Add($"MENU01.SFD", $"Video: Main Menu, Normal Race Preview");
        NameToDescription.Add($"MENU02.SFD", $"Video: Main Menu, Heroes Story Preview");
        NameToDescription.Add($"MENU03.SFD", $"Video: Main Menu, Babylon Story Preview");
        NameToDescription.Add($"MENU04.SFD", $"Video: Main Menu, Mission Mode Preview");
        NameToDescription.Add($"MENU05.SFD", $"Video: Main Menu, Survival Race Preview");
        NameToDescription.Add($"MENU06.SFD", $"Video: Main Menu, Survival Battle Preview");

        NameToDescription.Add($"TITLE_X.SFD", $"Video: Title Screen Intro");
        NameToDescription.Add($"STORY_E.SFD", $"Video: Story Cutscene, Post Sand Ruins");
        NameToDescription.Add($"STORY.SFD", $"Video: Story Cutscene, Post Sand Ruins [Japanese]");
        NameToDescription.Add($"ENDING_E.SFD", $"Video: Story Cutscene, Story Ending");
        NameToDescription.Add($"ENDING.SFD", $"Video: Story Cutscene, Story Ending [Japanese]");
        NameToDescription.Add($"OPENING_E.SFD", $"Video: Story Cutscene, Story Opening");
        NameToDescription.Add($"OPENING.SFD", $"Video: Story Cutscene, Story Opening [Japanese]");

        NameToDescription.Add($"TUTO_X_E.SFD", $"Video: Tutorial");
        NameToDescription.Add($"TUTO_X_J.SFD", $"Video: Tutorial [Japanese]");

        // Speech
        NameToDescription.Add($"VOICE_E.AFS", $"Speech Archive: Character Lose, Win, Select Voices");
        NameToDescription.Add($"VOICE.AFS", $"Speech Archive: Character Lose, Win, Select Voices [Japanese]");
        NameToDescription.Add($"V_CHAO_E.AFS", $"Speech Archive: Omochao Announcer");
        NameToDescription.Add($"V_CHAO_J.AFS", $"Speech Archive: Omochao Announcer [Japanese]");

        // Music
        void AddCutsceneAudio(string path, string desc)
        {
            NameToDescription.Add(path, desc);
            NameToDescription.Add(Path.ChangeExtension(path, "aix"), desc + " [Multi-track/Surround]");
        }

        AddCutsceneAudio($"INSERT_J.adx", $"Copy of Story Cutscene Audio: Post Sand Ruins [Japanese]");
        AddCutsceneAudio($"INSERT_E.adx", $"Copy of Story Cutscene Audio: Post Sand Ruins");

        AddCutsceneAudio($"OP_J.adx", $"Copy of Story Cutscene Audio: Story Opening [Japanese]");
        AddCutsceneAudio($"OP_E.adx", $"Copy of Story Cutscene Audio: Story Opening");

        AddCutsceneAudio($"ED_J.adx", $"Copy of Story Cutscene Audio: Story Ending [Japanese]");
        AddCutsceneAudio($"ED_E.adx", $"Copy of Story Cutscene Audio: Story Ending");
        AddCutsceneAudio($"TUTO.adx", $"Copy of Video Audio: Tutorial");

        NameToDescription.Add($"SV1.adx", $"Stage Music: Survival Mode");

        NameToDescription.Add($"S20.adx", $"Music: Main Menu");
        NameToDescription.Add($"S21.adx", $"Music: Options Menu");
        NameToDescription.Add($"S22.adx", $"Music: Hang On");
        NameToDescription.Add($"S23.adx", $"Music: Super Hang On");
        NameToDescription.Add($"S24.adx", $"Music: Intro Cutscene: Super Sonic Riders");
        NameToDescription.Add($"S25.adx", $"Music: Theme of Heroes: High Flyin' Groove");
        NameToDescription.Add($"S26.adx", $"Music: Theme of Babylon: Catch Me if You Can");

        NameToDescription.Add($"S30.adx", $"Race Event Music: Race Complete");
        NameToDescription.Add($"S31.adx", $"Race Event Music: Speed Shoes");
        NameToDescription.Add($"S32.adx", $"Race Event Music: Invincibility");
        NameToDescription.Add($"S33.adx", $"Race Event Music: Level Down");
        NameToDescription.Add($"S34.adx", $"Race Event Music: Level Up");
        NameToDescription.Add($"S35.adx", $"Race Event Music: Go To Heaven (Digital Dimension)");
        NameToDescription.Add($"S36.adx", $"Race Event Music: Emergency Alert [Japanese]");
        NameToDescription.Add($"S37.adx", $"Race Event Music: Emergency Alert [English]");

        NameToDescription.Add($"S38.adx", $"Race Event Music: Lap Complete, Samba De Amigo! [Sega Carnival]");
        NameToDescription.Add($"S39.adx", $"Race Event Music: Landing on Battleship [Sega Carnival]");
        NameToDescription.Add($"S40.adx", $"Race Event Music: Enter QTE (Press Left + Right) Section [Sega Carnival]");
        NameToDescription.Add($"S41.adx", $"Race Event Music: Enter Monkey Ball Section [Sega Carnival]");
        NameToDescription.Add($"S42.adx", $"Race Event Music: Enter Crazy Taxi Section [Sega Carnival]");

        NameToDescription.Add($"S43.adx", $"Race Event Music: Enter Space Channel 5 Section [Sega Illusion]");
        NameToDescription.Add($"S44.adx", $"Race Event Music: Enter Chu Chu Rocket (Final) Section [Sega Illusion]");

        NameToDescription.Add($"S45.adx", $"Race Event Music: Babylon Guardian ??");
        NameToDescription.Add($"S46.adx", $"Race Event Music: Babylon Guardian Start Attack");
        NameToDescription.Add($"S47.adx", $"Race Event Music: Babylon Guardian Defeated");
        NameToDescription.Add($"S48.adx", $"Race Event Music: Babylon Guardian ??");

        NameToDescription.Add($"S60.adx", $"Story Music: Eggman Again, From First Story Cutscene");
        NameToDescription.Add($"S61.adx", $"Story Music: Legend of Babylonians");
        NameToDescription.Add($"S62.adx", $"Story Music: Rise of Babylon Garden");
        NameToDescription.Add($"S63.adx", $"Story Music: The Real Treasure");

        // Menus 
        NameToDescription.Add($"TSTS", $"Menu: Title Screen");
        NameToDescription.Add($"TSC", $"Menu: Common Assets");
        NameToDescription.Add($"STORY", $"Menu: Story Load Screen");
        NameToDescription.Add($"STAFF", $"Menu: Credits");
        NameToDescription.Add($"EVDM", $"Menu: Demo");

        foreach (var langNamePair in _languageNamePairs)
        {
            NameToDescription.Add($"MEXP{langNamePair.fileName}", $"Menu: Mission Mode UI [{langNamePair.desc}]");
            NameToDescription.Add($"TSEX{langNamePair.fileName}", $"Menu: Extras [{langNamePair.desc}]");
            NameToDescription.Add($"TSMS{langNamePair.fileName}", $"Menu: Options [{langNamePair.desc}]");
            NameToDescription.Add($"TSGM{langNamePair.fileName}", $"Menu: Main Menu [{langNamePair.desc}]");
            NameToDescription.Add($"GTS{langNamePair.fileName}", $"Menu: Race End Goal [{langNamePair.desc}]");

            NameToDescription.Add($"GMF{langNamePair.fileName}", $"Menu: Mission Mode (Pause?) Text [{langNamePair.desc}]");
            NameToDescription.Add($"GNF{langNamePair.fileName}", $"Menu: Normal Race (Pause?) Text [{langNamePair.desc}]");
            NameToDescription.Add($"MT{langNamePair.fileName}", $"Menu: Mission Mode Results [{langNamePair.desc}]");
            NameToDescription.Add($"LK{langNamePair.fileName}", $"Menu: Unknown Loading Screen [{langNamePair.desc}]");
        }

        NameToDescription.Add($"MB", $"Unknown Collection of Sega NN Skeletal Animations");
        NameToDescription.Add($"ME", $"Unknown Collection of Sega NN Skeletal Animations");
        NameToDescription.Add($"MO", $"Unknown Collection of Sega NN Skeletal Animations");
        NameToDescription.Add($"MS", $"Unknown Collection of Sega NN Skeletal Animations");
        NameToDescription.Add($"AS", $"Menu: Memory Card");
    }

    public static string TryDescribe(string fileName)
    {
        NameToDescription.TryGetValue(fileName, out var value);
        return value;
    }
}