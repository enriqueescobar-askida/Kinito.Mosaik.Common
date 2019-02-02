// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITranslator.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the ITranslator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text;

namespace Alphamosaik.Oceanik.Sdk
{
    public interface ITranslator
    {
        string Translate(string source, string languageDestination);

        string Translate(StringBuilder source, string languageDestination);

        int GetLcid(string languageCode);

        string GetLanguageCode(string lcid);

        string GetLanguageCode(int lcid);

        string GetCurrrentLanguageCode();

        string GetWebPartLanguage(System.Web.UI.WebControls.WebParts.WebPart webPart);
    }
}
