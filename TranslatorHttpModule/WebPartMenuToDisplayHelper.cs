// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebPartMenuToDisplayHelper.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the WebPartMenuToDisplayHelper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace TranslatorHttpHandler
{
    public class WebPartMenuToDisplayHelper
    {
        private readonly bool _itemLanguageEnabled;
        private readonly bool _contentWebpartEnabled;
        private readonly string _language;
        private readonly Guid _storageKey;

        public WebPartMenuToDisplayHelper(bool itemLanguageEnabled, bool contentWebpartEnabled, string language, Guid storageKey)
        {
            _itemLanguageEnabled = itemLanguageEnabled;
            _storageKey = storageKey;
            _language = language;
            _contentWebpartEnabled = contentWebpartEnabled;
        }

        public Guid StorageKey
        {
            get { return _storageKey; }
        }

        public string Language
        {
            get { return _language; }
        }

        public bool ContentWebpartEnabled
        {
            get { return _contentWebpartEnabled; }
        }

        public bool ItemLanguageEnabled
        {
            get { return _itemLanguageEnabled; }
        }
    }
}
