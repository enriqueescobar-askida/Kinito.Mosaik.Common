// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LanguageItem.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the LanguageItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Translator.Common.Library
{
    public class LanguageItem
    {
        public LanguageItem(string languageCode, int lcid, string displayName, string picture, bool visible)
        {
            LanguageDestination = languageCode;
            Picture = picture;
            DisplayName = displayName;
            Lcid = lcid;
            Visible = visible;
        }

        public string Picture { get; private set; }

        public string DisplayName { get; private set; }

        public int Lcid { get; private set; }

        public string LanguageDestination { get; private set; }

        public bool Visible { get; private set; }
    }
}
