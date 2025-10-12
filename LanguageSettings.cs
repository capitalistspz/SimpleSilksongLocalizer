using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TeamCherry.Localization;

namespace SimpleSilksongLocalizer;

internal class LanguageSettings
{
    public class FallbackSettings
    {
        [JsonProperty(Required = Required.Always)]
        public string Language { get; set; }
        
        [JsonProperty(Required = Required.AllowNull)]
        public string[] Excluded { get; set; }
    };
    
    [JsonProperty(Required = Required.AllowNull)]
    public string[]? CustomSheetTitles { get; set; }
    
    [JsonProperty(Required = Required.AllowNull)]
    public string[]? CustomLanguages { get; set; }
    
    [JsonProperty(Required = Required.AllowNull)]
    public FallbackSettings? Fallback { get; set; }

    public string GetFallbackLang(string sourceLanguage)
    {
        if (!SimpleSilksongLocalizerPlugin.EnableFallbacks.Value || Fallback == null || Fallback.Excluded.Contains(sourceLanguage))
            return String.Empty;
        var fallbackLang = Fallback.Language;
        return !Enum.TryParse(fallbackLang, out LanguageCode _) ? String.Empty : fallbackLang;
    }
}