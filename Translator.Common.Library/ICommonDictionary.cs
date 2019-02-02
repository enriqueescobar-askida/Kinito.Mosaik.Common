// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICommonDictionary.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the ICommonDictionary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using Microsoft.SharePoint;

namespace Translator.Common.Library
{
    using System;

    public interface ICommonDictionary
    {
        bool Translate(Guid webApplicationId, Guid siteId, Guid webId, string text, string languageSource, string languageDestination, out string translated);

        bool Translate(int hashCode, Guid webApplicationId, Guid siteId, Guid webId, string text, string languageSource, string languageDestination, out string translated);

        bool ContainText(string text, string languageSource, string languageDestination, out string translated);

        bool ContainItem(int id, string text, string languageSource, string languageDestination, out string translated);

        void Load(SPList list, string query, IEnumerable<CultureInfo> cultures);
    }
}
