using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using TeamCherry.Localization;

namespace SimpleSilksongLocalizer;

public partial class SimpleSilksongLocalizerPlugin : BaseUnityPlugin
{
    internal static List<string> ModLanguageDirectories = [];
    internal static HashSet<string> ModRegisteredSheetTitles = [];
    internal static Dictionary<LanguageCode, Dictionary<string,Dictionary<string, string>>> ModExtraEntries = new ();
    
    private Harmony harmony;
    private string[] originalSheetTitles;
    
    private void Awake()
    {
        harmony = new Harmony(Id);
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }
    private void Start()
    {
        // For initial load, present because patching `Language` at `Awake` or `OnEnable` causes a crash due to its static initializer
        Apply();
    }

    private void OnEnable()
    {
        // For reloads
        if (!didStart)
            return;
        Apply();
    }

    private void OnDisable()
    {
        Unapply();
    }

    private void Apply()
    {
        originalSheetTitles = Language._settings.sheetTitles;
        ArrayUtils.Add(ref Language._settings.sheetTitles, ModRegisteredSheetTitles.ToArray());
        
        harmony.PatchAll(typeof(LanguagePatch));
        
        Language.LoadLanguage();
    }

    private void Unapply()
    {
        harmony.UnpatchSelf();
        Language._settings.sheetTitles = originalSheetTitles;
        Language.LoadLanguage();
    }
}