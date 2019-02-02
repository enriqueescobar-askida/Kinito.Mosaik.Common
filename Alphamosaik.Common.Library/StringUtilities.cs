// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringUtilities.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the StringUtilities type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Alphamosaik.Common.Library
{
    public class StringUtilities
    {
        public static int GetHashCode(string text)
        {
            return text.GetHashCode();
        }

        public static int CharacterCounter(string text, string character, StringComparison comparisonType)
        {
            int textLength = text.Length;
            int characterLength = character.Length;
            int index = 0;
            int count = 0;

            if (!string.IsNullOrEmpty(character))
            {
                while (index < textLength && (index = text.IndexOf(character, index, comparisonType)) != -1)
                {
                    count++;
                    index += characterLength;
                }
            }

            return count;
        }

        public static int CharacterCounter(StringBuilder text, string character, StringComparison comparisonType)
        {
            int textLength = text.Length;
            int characterLength = character.Length;
            int index = 0;
            int count = 0;

            if (!string.IsNullOrEmpty(character))
            {
                while (index < textLength && (index = text.IndexOf(character, index, comparisonType)) != -1)
                {
                    count++;
                    index += characterLength;
                }
            }

            return count;
        }

        public static string EncodeToBase64(string str)
        {
            byte[] encbuff = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }

        public static string DecodeFromBase64(string str)
        {
            byte[] decbuff = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(decbuff);
        }

        public static string Crypt(string data, string password, bool encrypt)
        {
            var salt = new byte[] { 0x26, 0x19, 0x81, 0x4E, 0xA0, 0x6D, 0x95, 0x34, 0x26, 0x75, 0x64, 0x05, 0xF6 };

            var passwordDeriveBytes = new PasswordDeriveBytes(password, salt);

            var alg = Rijndael.Create();
            alg.Key = passwordDeriveBytes.GetBytes(32);
            alg.IV = passwordDeriveBytes.GetBytes(16);

            var cryptoTransform = encrypt ? alg.CreateEncryptor() : alg.CreateDecryptor();

            var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
            {
                byte[] bytes = encrypt ? Encoding.Unicode.GetBytes(data) : Convert.FromBase64String(data);

                try
                {
                    cryptoStream.Write(bytes, 0, bytes.Length);
                    cryptoStream.Close();
                }
                catch
                {
                    return null;
                }

                return encrypt ? Convert.ToBase64String(memoryStream.ToArray()) : Encoding.Unicode.GetString(memoryStream.ToArray());
            }
        }
    }
}
