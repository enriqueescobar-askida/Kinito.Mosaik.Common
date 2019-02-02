// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Languages.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the Languages type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Alphamosaik.Common.SharePoint.Library;

namespace Translator.Common.Library
{
    using System.Globalization;

    public class Languages : BaseStaticOverride<Languages>
    {
        private readonly string[] _allLanguages = new[]
                                                         {
                                                             "EN", "FR", "ES", "KO", "AR", "JP", "CH", "PT", "DE", "IT",
                                                             "RU", "DU", "UK", "PL", "EU", "BG", "CA", "ZH", "HR", "CS",
                                                             "DA", "ET", "FI", "GL", "EL", "IW", "HI", "HU", "KK", "LV",
                                                             "LT", "NO", "BR", "RO", "SR", "SK", "SL", "SV", "TH", "TR"
                                                         };

        private readonly string[] _lcid = new[]
                                                   {
                                                       "1033", "1036", "3082", "1042", "1025", "1041", "2052", "2070", "1031", "1040", 
                                                       "1049", "1043", "1058", "1045", "1069", "1026", "1027", "1028", "1050", "1029", 
                                                       "1030", "1061", "1035", "1110", "1032", "1037", "1081", "1038", "1087", "1062", 
                                                       "1063", "1044", "1046", "1048", "2074", "1051", "1060", "1053", "1054", "1055"
                                                   };

        private readonly Dictionary<string, string> _languageCodeToLcid = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _lcidTolanguageCode = new Dictionary<string, string>();

        public Languages()
        {
            for (int i = 0; i < AllLanguages.Length; i++)
            {
                _languageCodeToLcid.Add(AllLanguages[i], Lcid[i]);
                _lcidTolanguageCode.Add(Lcid[i], AllLanguages[i]);
            }
        }

        public string[] AllLanguages
        {
            get { return _allLanguages; }
        }

        public string[] Lcid
        {
            get { return _lcid; }
        }

        public int GetLcid(string languageCode)
        {
            string value;
            _languageCodeToLcid.TryGetValue(languageCode, out value);

            return Convert.ToInt32(value);
        }

        public string GetLanguageCode(string lcid)
        {
            string value;
            _lcidTolanguageCode.TryGetValue(lcid, out value);

            return value;
        }

        public string GetLanguageCode(int lcid)
        {
            string value;
            _lcidTolanguageCode.TryGetValue(lcid.ToString(), out value);

            return value;
        }

        public string GetBackwardCompatibilityLanguageCode(string languageCode)
        {
            if (languageCode.IndexOf("NL", StringComparison.OrdinalIgnoreCase) != -1)
            {
                return "DU";
            }

            return languageCode;
        }

        public string GetBackwardCompatibilityLanguageCode(int lcid)
        {
            if (lcid == 1043)
            {
                return "DU";
            }

            return GetLanguageCode(lcid);
        }

        public bool IsSupportedLanguage(string languageCode)
        {
            return _languageCodeToLcid.ContainsKey(languageCode);
        }

        public string GetIsoThreeLettersLanguage(string languageCode)
        {
            return GetIsoThreeLettersLanguage(GetLcid(languageCode));
        }

        public string GetIsoThreeLettersLanguage(int lcid)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(lcid);

            return cultureInfo.ThreeLetterISOLanguageName;
        }
    }
}