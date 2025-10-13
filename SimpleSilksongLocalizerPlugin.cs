using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using TeamCherry.Localization;

namespace SimpleSilksongLocalizer;

[BepInAutoPlugin(id: "capitalistspz.simplesilksonglocalizer")]
public partial class SimpleSilksongLocalizerPlugin
{
    /// <summary>
    /// Register sheet to be used
    /// Any sheets that are not already registered will not be used by the game
    /// </summary>
    public void RegisterSheet(string sheetTitle)
    {
        ModCustomSheetTitles.Add(sheetTitle);
    }
    /// <summary>
    /// Add a directory in which sheets are located
    /// </summary>
    public static void AddLanguageDirectory(string sheetDirectory)
    {
        var sheetDirInfo = new DirectoryInfo(sheetDirectory);
        var settingsFileInfo = sheetDirInfo
            .EnumerateFiles("LanguageSettings.json")
            .FirstOrDefault();
        if (settingsFileInfo == null)
            return;
        var settingsFilePath = settingsFileInfo.FullName;
        try
        {
            var settings =
                Newtonsoft.Json.JsonConvert.DeserializeObject<LanguageSettings>(
                    File.ReadAllText(settingsFilePath));
            if (settings == null)
                return;
            ModLanguageDirectories.Add(sheetDirectory, settings);
        }
        catch (Newtonsoft.Json.JsonSerializationException)
        {
            Logger.LogError($"Failed to parse language settings at '{settingsFilePath}'");
        }
        
    }
    
    public static void AddSheet(LanguageCode languageCode, string sheet, Dictionary<string, string> entries)
    {
        var sheetDict = ModExtraEntries.GetOrInsertNew(languageCode).GetOrInsertNew(sheet);
        foreach (var (key, value) in entries)
        {
            sheetDict[key] = value;
        }
    }

    public static void AddSheetEntry(LanguageCode languageCode, string sheet, string key, string value)
    {
        var sheetDict = ModExtraEntries.GetOrInsertNew(languageCode).GetOrInsertNew(sheet);
        sheetDict[key] = value;
    }
}