// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateWrapperForTagDatePicker.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DelegateWrapperForTagDatePicker type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace TranslatorHttpHandler.MatchEvaluators
{
    public class DelegateWrapperForTagDatePicker : DelegateWrapperForTagsBase
    {
        public override string MatchEvaluatorTag(Match match)
        {
            string day = match.Groups["day"].Value.Trim();

            if (!string.IsNullOrEmpty(day))
            {
                string header = day[0].ToString();

                return match.Value.Replace(match.Value, "<th scope=\"col\" class=ms-picker-dayheader nowrap><ABBR title= \"" + day + "\" >&nbsp;" + header + "&nbsp;</ABBR></th>");
            }

            return match.Value;
        }
    }
}
