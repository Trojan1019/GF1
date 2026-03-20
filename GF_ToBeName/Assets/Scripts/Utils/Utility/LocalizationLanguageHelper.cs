using System.Collections.Generic;
using GameFramework.Localization;

namespace NewSideGame
{
    public static class LocalizationLanguageHelper
    {
        // UI 文本文件名：Assets/Game/Localization/uiText_{suffix}.csv
        // 注意：Language.Indonesian 的文件后缀是 "id"（不是 "Indonesian"），因此需要映射。
        private static readonly Dictionary<Language, string> LanguageToSuffix = new Dictionary<Language, string>
        {
            { Language.English, "English" },
            { Language.ChineseSimplified, "ChineseSimplified" },
            { Language.ChineseTraditional, "ChineseTraditional" },
            { Language.Japanese, "Japanese" },
            { Language.Korean, "Korean" },
            { Language.Vietnamese, "Vietnamese" },
            { Language.Spanish, "Spanish" },
            { Language.Russian, "Russian" },
            { Language.Italian, "Italian" },
            { Language.French, "French" },
            { Language.German, "German" },
            { Language.Thai, "Thai" },
            //{ Language.Hindi, "Hindi" },
            { Language.Turkish, "Turkish" },
            { Language.PortuguesePortugal, "PortuguesePortugal" },
            { Language.Indonesian, "id" },
        };

        public static List<Language> SupportedLanguages
        {
            get
            {
                return new List<Language>(LanguageToSuffix.Keys);
            }
        }

        public static bool TryGetSuffix(Language language, out string suffix)
        {
            return LanguageToSuffix.TryGetValue(language, out suffix);
        }

        public static string GetAssetPath(Language language)
        {
            if (!TryGetSuffix(language, out var suffix))
                return null;

            return $"Assets/Game/Localization/uiText_{suffix}.csv";
        }
    }
}

