// --------------------------------------------------------------------------------------------------------------------
// <copyright file="License.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the License type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Text;

namespace Alphamosaik.Common.Library.Licensing
{
    public class License
    {
        public static string PassPhrase = "TRECREGA2HUFRUP39CR7CH7CHATECHASWU2UF8NUZ6KU7EMAJAVECRES8UB6USTE";

        private const string TagAlphamosaik = "AlphaMosaik";
        private const string Version = "1.0";
        private const int DatasPosition = 512;

        public License()
        {
            IsValide = false;
            TrialStart = DateTime.Now;
            TrialEnd = TrialStart;
            Type = LicenseType.Unlicensed;
        }

        public License(string clientPrivateKey, string license)
        {
            IsValide = false;
            TrialStart = DateTime.Now;
            TrialEnd = TrialStart;
            Type = LicenseType.Unlicensed;

            try
            {
                string actualDecrypted = StringUtilities.Crypt(license, clientPrivateKey, false);

                int indexFirstTag = actualDecrypted.IndexOf(TagAlphamosaik);
                int indexLastTag = actualDecrypted.LastIndexOf(TagAlphamosaik);

                if (indexFirstTag > -1 && indexLastTag > -1)
                {
                    string stringDatas = actualDecrypted.Substring(indexFirstTag, (indexLastTag + TagAlphamosaik.Length) - indexFirstTag);

                    string[] datas = stringDatas.Split('&');

                    string version = datas[1];

                    if (version.Equals(Version))
                    {
                        Type = (LicenseType)Enum.Parse(typeof(LicenseType), datas[2]);

                        if (Type >= LicenseType.Trial && Type <= LicenseType.Unlicensed)
                        {
                            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

                            DateTime dateValue;
                            if (DateTime.TryParseExact(datas[3], "d", culture, DateTimeStyles.None, out dateValue))
                            {
                                TrialStart = dateValue;

                                if (DateTime.TryParseExact(datas[4], "d", culture, DateTimeStyles.None, out dateValue))
                                {
                                    TrialEnd = dateValue;

                                    IsValide = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        public enum LicenseType
        {
            /// <summary>
            /// Indicate it's a trial version
            /// </summary>
            Trial,

            /// <summary>
            /// Indicate it's a development version
            /// </summary>
            Dev,

            /// <summary>
            /// Indicate it's a production version
            /// </summary>
            Prod,

            /// <summary>
            /// Indicate it's a unlicensed version
            /// </summary>
            Unlicensed
        }

        public LicenseType Type { get; set; }

        public DateTime TrialStart { get; set; }

        public DateTime TrialEnd { get; set; }

        public bool IsValide { get; private set; }

        public string GenerateLicense(string clientPrivateKey)
        {
            var loremIpsumWords = new StringBuilder(LoremIpsumGenerator.GetWords(512));

            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

            string datas = String.Format("{0}&{1}&{2}&{3}&{4}&{5}", TagAlphamosaik, Version, Type, TrialStart.ToString("d", culture), TrialEnd.ToString("d", culture), TagAlphamosaik);

            loremIpsumWords.Remove(DatasPosition, datas.Length).Insert(DatasPosition, datas);

            return StringUtilities.Crypt(loremIpsumWords.ToString(), clientPrivateKey, true);
        }
    }
}
