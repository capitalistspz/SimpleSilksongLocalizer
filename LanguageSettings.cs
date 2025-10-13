using System;
using System.Linq;
using Newtonsoft.Json;
using TeamCherry.Localization;

namespace SimpleSilksongLocalizer;

internal class LanguageSettings
{
    internal class FallbackSettings
    {
        [JsonProperty(Required = Required.Always)]
        public string Language { get; set; } = null!;

        [JsonProperty(Required = Required.Default)]
        public string[]? Excluded { get; set; }
    };
    
    [JsonProperty(Required = Required.Default)]
    public string[]? CustomSheetTitles { get; set; }
    
    [JsonProperty(Required = Required.Default)]
    public string[]? CustomLanguages { get; set; }
    
    [JsonProperty(Required = Required.Default)]
    public FallbackSettings? Fallback { get; set; }

    public string GetFallbackLang(string sourceLanguage)
    {
        if (!SimpleSilksongLocalizerPlugin.EnableFallbacks.Value || Fallback == null || 
            (Fallback.Excluded != null && Fallback.Excluded.Contains(sourceLanguage)))
            return String.Empty;
        var fallbackLang = Fallback.Language;
        return !Enum.TryParse(fallbackLang, out LanguageCode _) ? String.Empty : fallbackLang;
    }
}