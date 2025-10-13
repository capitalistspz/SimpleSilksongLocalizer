using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TeamCherry.Localization;

namespace SimpleSilksongLocalizer;

internal class LanguageSettings
{
    internal class FallbackSettings
    {
        [JsonProperty(Required = Required.Always), JsonConverter(typeof(StringEnumConverter))]
        public LanguageCode Language { get; set; }

        [JsonProperty(Required = Required.Default)]
        public string[]? Excluded { get; set; }
    };
    
    [JsonProperty(Required = Required.Default)]
    public string[]? CustomSheetTitles { get; set; }
    
    [JsonProperty(Required = Required.Default, ItemConverterType = typeof(StringEnumConverter))]
    public LanguageCode[]? CustomLanguages { get; set; }
    
    [JsonProperty(Required = Required.Default)]
    public FallbackSettings? Fallback { get; set; }

    public string GetFallbackLang(string sourceLanguage)
    {
        if (!SimpleSilksongLocalizerPlugin.EnableFallbacks.Value || Fallback == null || 
            (Fallback.Excluded != null && Fallback.Excluded.Contains(sourceLanguage)))
            return String.Empty;
        var fallbackLang = Fallback.Language;
        return fallbackLang != LanguageCode.N ? String.Empty : fallbackLang.ToString();
    }
}