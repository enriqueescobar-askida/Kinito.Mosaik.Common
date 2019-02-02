// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForTagInnerHtml.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForTagInnerHtml type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text;
using System.Text.RegularExpressions;
using Alphamosaik.Common.Library;

namespace TranslatorHttpHandler.MatchEvaluators
{
    public class DelegateWrapperForTagInnerHtml : DelegateWrapperForTagsBase
    {
        private readonly StringBuilder _valueGlobal = new StringBuilder();

        private static readonly string[] Characters = new[] 
            { 
              "\\u00DC", "\\u00C4", "\\u00D6", "\\u00CB", "\\u00C7", "\\u00C6", "\\u00C5", "\\u00D8", 
              "\\u00FC", "\\u00E4", "\\u00F6", "\\u00EB", "\\u00E7", "\\u00E5", "\\u00F8", "\\u0060", "\\u00E0", "\\u00E8", "\\u00EC", "\\u00F2", "\\u00F9", "\\u0025", 
              "\\u0026", "\\u00DF", "\\u00A0", "\\u003e", "\\u003c",  
              "\\u009A", "\\u00A2", "\\u00A3", "\\u00AB", "\\u00BB", "\\u00AE", "\\u00AD", 
              "\\u00E1", "\\u00FA", "\\u00F3", "\\u00E9", "\\u00ED", "\\u00F1", "\\u00A7", "\\u00E8", "\\u00EE", "\\u00F4", "\\u00E2", "\\u00FB", "\\u00EA", 
              "\\u00E6", "\\u00A1", "\\u0022", "\\u00AA", "\\u00D7", "\\u00B0", "\\u20AC", "\\u007C", "\\u00B5", "\\u00BA", "\\u00F7", "\\u00B2", "\\u00B3", 
              "\\u00B1", "&quot;", "\\u002f", "\\u0026"
            };

        private static readonly string[] Entities = new[] 
            { 
               "&Uuml;", "&Auml;", "&Ouml;", "&Euml;", "ç;", "&AElig;", "&Aring;", "&Oslash;", 
               "&uuml;", "&auml;", "&ouml;", "&euml;", "ç", "&aring;", "&oslash;", "&grave;", "à", "è", "&igrave;", "&ograve;", "ù", "%", 
               "&", "&szlig;", "&nbsp;", ">", "<", 
               "&copy;", "&cent;", "&pound;", "&laquo;", "&raquo;", "&reg;", "&middot;", 
               "&aacute;", "&uacute;", "&oacute;", "é", "&iacute;", "&ntilde;", "&sect;", "è", "î", "ô", "â", "û", "ê", 
               "&aelig;", "&iexcl;", "&quot;", "&ordf;", "&times;", "&deg;", "€", "&brvbar;", "µ", "°", "÷", "&sup2;", "&sup3;", 
               "&plusmn;", "\"", "/", "&"
            };

        public override string MatchEvaluatorTag(Match match)
        {
            _valueGlobal.SetNewString(match.Value);

            MatchCollection result = TranslatorRegex.GreaterThanSmallerThanRegex.Matches(match.Value.Replace("\\u003c", "<").Replace("\\u003e", ">"));

            foreach (Match currentMatch in result)
            {
                var value = new StringBuilder(currentMatch.Value.Substring(1, currentMatch.Value.Length - 2).Replace("&quot;", "\"").Replace("&lt;", "<").Replace("&gt;", ">").Trim());

                for (int i = 0; i < Characters.Length; i++)
                {
                    value.Replace(Characters[i].ToLower(), Entities[i].ToLower());
                }

                value.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&nbsp;", " ").Replace("&amp;", "&");

                if (!StringBuilderExtensions.IsNullOrEmpty(value))
                {
                    string currentValue = value.ToString();

                    string translated;
                    if (Dictionary.Translate(CurrentContext, currentValue, LanguageSource, LanguageDestination, out translated))
                    {
                        var textToReplace = new StringBuilder(currentMatch.Value.Substring(1, currentMatch.Value.Length - 2));

                        for (int i = 0; i < Characters.Length; i++)
                        {
                            textToReplace.Replace(Entities[i], Characters[i]);
                        }

                        _valueGlobal.Replace("\\u003e" + textToReplace + "\\u003c", "\\u003e" + translated.Replace("\"", "\\u0026quot;").Replace("<", "&lt;").Replace(">", "&gt;") + "\\u003c");
                    }
                }
            }

            return _valueGlobal.ToString();
        }
    }
}
