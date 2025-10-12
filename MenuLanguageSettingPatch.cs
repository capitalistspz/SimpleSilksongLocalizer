using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using GlobalEnums;
using HarmonyLib;
using TeamCherry.Localization;
using UnityEngine.UI;

namespace SimpleSilksongLocalizer;

public class MenuLanguageSettingPatch
{
    [HarmonyPatch(typeof(MenuLanguageSetting), nameof(MenuLanguageSetting.RefreshAvailableLanguages))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> SupportAllAvailableLanguages(IEnumerable<CodeInstruction> instructions)
    {
        var del = Transpilers.EmitDelegate<Func<SupportedLanguages[]>>(() =>
        {
            List<SupportedLanguages> outArr = [];
            foreach (var langString in Language._availableLanguages)
            {
                if (Enum.TryParse(langString, out LanguageCode lang))
                {
                    outArr.Add((SupportedLanguages)lang);
                }
            }
            return outArr.ToArray();
        });
        var match = new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldtoken, typeof(SupportedLanguages)),
                new CodeMatch(
                    OpCodes.Call,
                    AccessTools.Method(typeof(Type), nameof(Type.GetTypeFromHandle),
                        [typeof(RuntimeTypeHandle)])),
                new CodeMatch(OpCodes.Call,
                    AccessTools.Method(typeof(Enum), nameof(Enum.GetValues), [typeof(Type)])))
            .Repeat(matcher => matcher.RemoveInstructions(2).SetAndAdvance(del.opcode, del.operand));
        return match.InstructionEnumeration();
    }

    [HarmonyPatch(typeof(MenuLanguageSetting), nameof(MenuLanguageSetting.UpdateLangsArray))]
    [HarmonyPostfix]
    public static void UpdateLangsArrayStrings(ref string[] ___optionList, SupportedLanguages[] ___langs)
    {
        ___optionList = ___langs.Select(opt => ((LanguageCode)opt).ToString()).ToArray();
    }
}