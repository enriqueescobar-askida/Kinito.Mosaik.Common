// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrivateKey.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the PrivateKey type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Common.Library.Licensing
{
    public class PrivateKey
    {
        public static string GetPrivateKey()
        {
            string actual = UniqueIdentifier.GetIdentifier();

            string actualEncrypted = StringUtilities.Crypt(actual, License.PassPhrase, true);
            return StringUtilities.EncodeToBase64(actualEncrypted);
        }

        public static string GetPrivateKey(string productName)
        {
            string actual = UniqueIdentifier.GetIdentifier() + productName;

            string actualEncrypted = StringUtilities.Crypt(actual, License.PassPhrase, true);
            return StringUtilities.EncodeToBase64(actualEncrypted);
        }
    }
}
