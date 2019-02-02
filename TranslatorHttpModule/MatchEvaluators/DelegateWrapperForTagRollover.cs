// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForTagRollover.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForTagRollover type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Alphamosaik.Common.Library;

namespace TranslatorHttpHandler.MatchEvaluators
{
    public class DelegateWrapperForTagRollover : DelegateWrapperForTagsBase
    {
        private readonly StringBuilder _valueGlobal = new StringBuilder();

        private static readonly string[] Characters = new[] 
            { 
              "\\u00DC", "\\u00C4", "\\u00D6", "\\u00CB", "\\u00C7", "\\u00C6", "\\u00C5", "\\u00D8", 
              "\\u00FC", "\\u00E4", "\\u00F6", "\\u00EB", "\\u00E7", "\\u00E5", "\\u00F8", "\\u0060", "\\u00E0", "\\u00E8", "\\u00EC", "\\u00F2", "\\u00F9", "\\u0025", 
              "\\u0026", "\\u00DF", "\\u003e", "\\u003c", 
              "\\u009A", "\\u00A2", "\\u00A3", "\\u00AB\\u00A0", "\\u00A0\\u00BB", "\\u00AE", "\\u00AD", 
              "\\u00E1", "\\u00FA", "\\u00F3", "\\u00E9", "\\u00ED", "\\u00F1", "\\u00A7", "\\u00E8", "\\u00EE", 
              "\\u00F4", "\\u00E2", "\\u00FB", "\\u00EA", 
              "\\u00E6", "\\u00A1", "\\u0022", "\\u00AA", "\\u00D7", "\\u00B0", "\\u20AC", "\\u007C", "\\u00B5", "\\u00BA", "\\u00F7", "\\u00B2", "\\u00B3", 
              "\\u00B1", "&quot;", "\\u002f", "\\u0026", "\\u0027", "\\u0028", "\\u0029"
            };

        private static readonly string[] Entities = new[] 
            { 
               "&Uuml;", "&Auml;", "&Ouml;", "&Euml;", "ç;", "&AElig;", "&Aring;", "&Oslash;", 
               "&uuml;", "&auml;", "&ouml;", "&euml;", "ç", "&aring;", "&oslash;", "&grave;", "à", "è", "&igrave;", "&ograve;", "ù", "%", 
               "&", "&szlig;", ">", "<", 
               "&copy;", "&cent;", "&pound;", "« ", " »", "&reg;", "&middot;", 
               "&aacute;", "&uacute;", "&oacute;", "é", "&iacute;", "&ntilde;", "&sect;", "è", "î", "ô", "â", "û", "ê", 
               "&aelig;", "&iexcl;", "&quot;", "&ordf;", "&times;", "&deg;", "€", "&brvbar;", "µ", "°", "÷", "&sup2;", "&sup3;", 
               "&plusmn;", "\"", "/", "&", "'", "(", ")"
            };

        public override string MatchEvaluatorTag(Match match)
        {
            _valueGlobal.SetNewString(match.Value);

            MatchCollection result = TranslatorRegex.TagForRolloverRegex.Matches(match.Value);

            foreach (Match currentMatch in result)
            {
                string value = currentMatch.Value.Substring(3, currentMatch.Value.Length - 4).Trim().Replace("\\u0028", "(").Replace("\\u0029", ")");
                string titleValue = match.Value.Substring(22, match.Value.IndexOf("'", 22) - 22).Trim().Replace("\\u0028", "(").Replace("\\u0029", ")");

                for (int i = 0; i < Characters.Length; i++)
                {
                    value = new Regex(Characters[i].Replace("\\", "\\\\"), RegexOptions.IgnoreCase).Replace(value, Entities[i]);
                    titleValue = new Regex(Characters[i].Replace("\\", "\\\\"), RegexOptions.IgnoreCase).Replace(titleValue, Entities[i]);
                }

                if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(titleValue))
                {
                    string translatedTitle;
                    string translatedValue;
                    if (Dictionary.Translate(CurrentContext, value, LanguageSource, LanguageDestination, out translatedValue) && Dictionary.Translate(CurrentContext, titleValue, LanguageSource, LanguageDestination, out translatedTitle))
                    {
                        for (int i = 0; i < Characters.Length; i++)
                        {
                            translatedValue = translatedValue.Replace(Entities[i], Characters[i]);
                            translatedTitle = translatedTitle.Replace(Entities[i], Characters[i]);
                        }

                        _valueGlobal.Replace(match.Value, ";ShowListInformation('" + translatedTitle.Replace("'", "\\'") + "','" + translatedValue.Replace("'", "\\'") + "',");
                    }
                }
            }

            if (TranslatorRegex.DefinitionResetRegex.IsMatch(_valueGlobal.ToString()))
            {
                string value = TranslatorRegex.DefinitionResetRegex.Match(_valueGlobal.ToString()).Groups["value"].Value;
                string value2 = TranslatorRegex.DefinitionResetRegex.Match(_valueGlobal.ToString()).Groups["value2"].Value;

                string translated;
                if (!string.IsNullOrEmpty(value) && Dictionary.Translate(CurrentContext, value, LanguageSource, LanguageDestination, out translated))
                {
                    _valueGlobal.Replace(TranslatorRegex.DefinitionResetRegex.Match(_valueGlobal.ToString()).Value, TranslatorRegex.DefinitionResetRegex.Match(_valueGlobal.ToString()).Value.Replace(TranslatorRegex.DefinitionResetRegex.Match(_valueGlobal.ToString()).Groups["value"].Value, translated));
                }

                if (!string.IsNullOrEmpty(value2) && Dictionary.Translate(CurrentContext, value2, LanguageSource, LanguageDestination, out translated))
                {
                    _valueGlobal.Replace(TranslatorRegex.DefinitionResetRegex.Match(_valueGlobal.ToString()).Value, TranslatorRegex.DefinitionResetRegex.Match(_valueGlobal.ToString()).Value.Replace(TranslatorRegex.DefinitionResetRegex.Match(_valueGlobal.ToString()).Groups["value2"].Value, translated));
                }
            }

            foreach (Match currentMatch in TranslatorRegex.HidDescriptionRegex.Matches(_valueGlobal.ToString()))
            {
                string value = currentMatch.Groups["value"].Value;

                string translated;
                if (!string.IsNullOrEmpty(value) && Dictionary.Translate(CurrentContext, value, LanguageSource, LanguageDestination, out translated))
                {
                    _valueGlobal.Replace(currentMatch.Value, currentMatch.Value.Replace(value, translated));
                }
            }

            if (_valueGlobal.IndexOf("|") == 3)
            {
                string decodedUrl = DecodeUrl(_valueGlobal.ToString());
                _valueGlobal.Clear().Append(decodedUrl);

                foreach (Match currentMatch in TranslatorRegex.CallBackRegex.Matches(_valueGlobal.ToString()))
                {
                    string value = currentMatch.Groups["value"].Value;

                    string translated;
                    if (!string.IsNullOrEmpty(value) && Dictionary.Translate(CurrentContext, value, LanguageSource, LanguageDestination, out translated))
                    {
                        _valueGlobal.Replace(currentMatch.Value, currentMatch.Value.Replace(value, translated));
                    }
                }
            }

            return _valueGlobal.ToString();
        }

        private static string DecodeUrl(string url)
        {
            int iof = -1;

            iof = url.IndexOf("%", iof + 1);

            while (iof != -1)
            {
                int deci = int.Parse(url.Substring(iof + 1, 2), NumberStyles.HexNumber);
                url = url.Substring(0, iof) + char.ConvertFromUtf32(deci) + url.Substring(iof + 3);

                iof = url.IndexOf("%", iof + 1);
            }

            return url;
        }
    }
}
