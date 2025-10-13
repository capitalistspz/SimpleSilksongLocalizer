using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using TeamCherry.Localization;

namespace SimpleSilksongLocalizer;
public partial class SimpleSilksongLocalizerPlugin : BaseUnityPlugin
{
    internal static Dictionary<string, LanguageSettings?> ModLanguageDirectories = [];
    internal static List<string> ModCustomSheetTitles = [];
    //TODO: Make it actually possible to use custom languages without them having the General sheet
    internal static List<LanguageCode> ModCustomLanguages = [];
    internal static Dictionary<LanguageCode, Dictionary<string,Dictionary<string, string>>> ModExtraEntries = new ();
    internal static ConfigEntry<bool> EnableFallbacks;

    public static new ManualLogSource Logger;
    private Harmony harmony;
    private string[] originalSheetTitles;
    
    private void Awake()
    {
        Logger = base.Logger;

        harmony = new Harmony(Id);
        EnableFallbacks = Config.Bind("Fallbacks", "Enable", false, "Enable fallback strings for untranslated text, if the source has them enabled");
        
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }

    private void FindDirectories()
    {
        var pluginDir = new DirectoryInfo(BepInEx.Paths.PluginPath);
        foreach (var pluginSubDir in pluginDir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
        {
            foreach (var languageDir in pluginSubDir.EnumerateDirectories("Language", SearchOption.TopDirectoryOnly))
            {
                AddLanguageDirectory(languageDir.FullName);
            }
        }
    }
    
    private void Start()
    {
        FindDirectories();

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
        foreach (var setting in ModLanguageDirectories.Values)
        {
            if (setting == null)
                continue;
            if (setting.CustomSheetTitles != null)
                ModCustomSheetTitles.AddRange(setting.CustomSheetTitles);
            if (setting.CustomLanguages != null) 
                ModCustomLanguages.AddRange(setting.CustomLanguages);
        }
        
        originalSheetTitles = Language._settings.sheetTitles;
        
        Language._settings.sheetTitles = Enumerable.Union(Language._settings.sheetTitles, ModCustomSheetTitles).ToArray();
        
        harmony.PatchAll(typeof(LanguagePatch));
        harmony.PatchAll(typeof(MenuLanguageSettingPatch));
        
        Language.LoadAvailableLanguages();
        Language.LoadLanguage();
    }

    private void Unapply()
    {
        harmony.UnpatchSelf();
        
        Language._settings.sheetTitles = originalSheetTitles;
        Language.LoadAvailableLanguages();
        Language.LoadLanguage();
    }
}