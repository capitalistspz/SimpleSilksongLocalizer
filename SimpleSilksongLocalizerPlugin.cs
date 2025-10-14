using System;
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
        ModCustomSheets.Add(sheetTitle);
    }
    /// <summary>
    /// Add a directory in which sheets are located
    /// </summary>
    public static void AddLocalizationDirectory(string localizationDirectory)
    {
        var sheetDirInfo = new DirectoryInfo(localizationDirectory);
        var settingsFileInfo = sheetDirInfo
            .EnumerateFiles("LocalizationSettings.json")
            .FirstOrDefault();
        if (settingsFileInfo == null)
        {
            ModLocalizationDirectories.Add(localizationDirectory, null);
            return;
        }
        var settingsFilePath = settingsFileInfo.FullName;
        try
        {
            var settings =
                Newtonsoft.Json.JsonConvert.DeserializeObject<LocalizationSettings>(
                    File.ReadAllText(settingsFilePath));
            if (settings == null)
                return;
            ModLocalizationDirectories.Add(localizationDirectory, settings);
        }
        catch (Newtonsoft.Json.JsonSerializationException e)
        {
            Logger.LogError(
                $"Failed to parse language settings at '{settingsFilePath}': {e}");
        }
        catch (Exception e)
        {
            Logger.LogError($"Unexpected exception: {e}");
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