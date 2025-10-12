using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TeamCherry.Localization;

namespace SimpleSilksongLocalizer;

public partial class SimpleSilksongLocalizerPlugin : BaseUnityPlugin
{
    internal static List<string> ModLanguageDirectories = [];
    internal static List<string> ModRegisteredSheetTitles = [];
    internal static Dictionary<LanguageCode, Dictionary<string,Dictionary<string, string>>> ModExtraEntries = new ();

    public static new ManualLogSource Logger;
    private Harmony harmony;
    private string[] originalSheetTitles;
    
    private void Awake()
    {
        Logger = base.Logger;

        harmony = new Harmony(Id);
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }

    private void FindDirectories()
    {
        var pluginDir = new DirectoryInfo(BepInEx.Paths.PluginPath);
        foreach (var pluginSubDir in pluginDir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
        {
            foreach (var languageDir in pluginSubDir.EnumerateDirectories("Language", SearchOption.TopDirectoryOnly))
            {
                Logger.LogInfo($"Found language directory in {pluginSubDir.Name}");
                ModLanguageDirectories.Add(languageDir.FullName);
            }
        }
    }
    
    private void Start()
    {
        // For initial load, present because patching `Language` at `Awake` or `OnEnable` causes a crash due to its static initializer
        Apply();
    }

    private void OnEnable()
    {
        // For reloads
        if (!didStart || harmony.GetPatchedMethods().Any())
            return;
        Apply();
    }

    private void OnDisable()
    {
        Unapply();
    }

    private void Apply()
    {
        FindDirectories();

        foreach (var line in ModLanguageDirectories
                     .Select(dir => Path.Combine(dir, "CustomSheetTitles.txt"))
                     .Where(File.Exists)
                     .SelectMany(File.ReadLines))
        {
            ModRegisteredSheetTitles.Add(line);
        }
        
        originalSheetTitles = Language._settings.sheetTitles;
        Language._settings.sheetTitles = Language._settings.sheetTitles.Union(ModRegisteredSheetTitles).ToArray();
        
        harmony.PatchAll(typeof(LanguagePatch));
        harmony.PatchAll(typeof(MenuLanguageSettingPatch));
        
        Language.LoadAvailableLanguages();
        Language.LoadLanguage();
    }

    private void Unapply()
    {
        harmony.UnpatchSelf();
        ModLanguageDirectories.Clear();
        
        Language._settings.sheetTitles = originalSheetTitles;
        Language.LoadAvailableLanguages();
        Language.LoadLanguage();
    }
}