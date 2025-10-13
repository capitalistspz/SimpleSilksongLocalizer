using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Xml;
using HarmonyLib;
using TeamCherry.Localization;
using TeamCherry.SharedUtils;
using UnityEngine;
using File = System.IO.File;

namespace SimpleSilksongLocalizer;

public static class LanguagePatch
{
    [HarmonyPatch(typeof(Language), nameof(Language.DoSwitch))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> SlackenXmlReader(IEnumerable<CodeInstruction> instructions)
    {
        var match = new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(XmlReader), nameof(XmlReader.Create), [typeof(TextReader)])));
        var newInstr = Transpilers.EmitDelegate<Func<StringReader, XmlReader>>(reader =>
        {
            var readerSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };
            return XmlReader.Create(reader, readerSettings);
        });
        match.Set(newInstr.opcode, newInstr.operand);
       return match.InstructionEnumeration();
    }

    [HarmonyPatch(typeof(Language), nameof(Language.DoSwitch))]
    [HarmonyPostfix]
    static void InsertCurrentLanguageExtraStrings()
    {
        if (SimpleSilksongLocalizerPlugin.ModExtraEntries.TryGetValue(Language._currentLanguage, out var modEntrySheets))
        {
            foreach (var (modSheet, modEntry) in modEntrySheets)
            {
                var gameEntries = Language._currentEntrySheets.GetOrInsertNew(modSheet);
                foreach (var (modKey, modValue) in modEntry)
                {
                    gameEntries[modKey] = modValue;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(Language), nameof(Language.HasLanguageFile))]
    [HarmonyPostfix]
    private static void HasModdedLanguageFile(ref bool __result, string lang, string sheetTitle)
    {
        if (__result)
            return;
        foreach (var (dir, settings) in SimpleSilksongLocalizerPlugin.ModLanguageDirectories)
        {
            var sheetPath = Path.Combine(dir, lang, sheetTitle);
            if (File.Exists(sheetPath))
            {
                __result = true;
                break;
            }

            var currentLang = Language._currentLanguage.ToString();
            var fallbackLang = settings.GetFallbackLang(currentLang);
            if (String.IsNullOrEmpty(fallbackLang) || fallbackLang == currentLang)
                continue;
            if (File.Exists(Path.Combine(dir, fallbackLang, sheetTitle)))
            {
                __result = true;
                break;
            }
        }
    }
    
    [HarmonyPatch(typeof(Language), nameof(Language.GetLanguageFileContents))]
    [HarmonyPostfix]
    private static void AddModdedLanguageFileContents(ref string __result, string sheetTitle)
    {
        var newResult = String.Empty;
        foreach (var (dir, settings) in SimpleSilksongLocalizerPlugin.ModLanguageDirectories)
        {
            var currentLang = Language._currentLanguage.ToString();
            var fallbackLang = settings.GetFallbackLang(currentLang);
            if (String.IsNullOrEmpty(fallbackLang) || fallbackLang == currentLang)
                continue;
            var fallbackPath = Path.Combine(dir, fallbackLang, sheetTitle);
            if (File.Exists(fallbackPath))
                newResult += File.ReadAllText(fallbackPath);
        }

        newResult += __result;
        
        foreach (var (dir, _) in SimpleSilksongLocalizerPlugin.ModLanguageDirectories)
        {
            var path = Path.Combine(dir, Language._currentLanguage.ToString(), sheetTitle);
            if (File.Exists(path))
            {
                newResult += File.ReadAllText(path);
            }
        }
        __result = newResult;
    }
}