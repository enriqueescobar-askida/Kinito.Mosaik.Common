// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Encryption.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the EncryptionUtility type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Alphamosaik.Oceanik.AutomaticTranslation
{
    using System.Collections.Generic;

    public static class EncryptionUtility
    {
        #region - Fields -
        private const string PassPhrase = "a#s5pr@se"; // can be any string
        private const string SaltValue = "s@1t45^lue"; // can be any string
        private const string HashAlgorithm = "SHA1"; // can be "MD5"
        private const int PasswordIterations = 2; // can be any number
        private const string InitVector = "@1B2c3D4e5F6g7H#"; // must be 16 bytes
        private const int KeySize = 256; // can be 192 or 128 
        #endregion

        /// <summary>
        /// Encrypts a specified string
        /// </summary>
        /// <param name="plainText">The string to be encrypted</param>
        /// <returns>The encrypted string</returns>
        public static string Encrypt(string plainText)
        {
            // Convert strings into byte arrays.
            // Let us assume that strings only contain ASCII codes.
            // If strings include Unicode characters, use Unicode, UTF7, or UTF8 
            // encoding.
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(InitVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(SaltValue);

            // Convert our plaintext into a byte array.
            // Let us assume that plaintext contains UTF8-encoded characters.
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            // First, we must create a password, from which the key will be derived.
            // This password will be generated from the specified passphrase and 
            // salt value. The password will be created using the specified hash 
            // algorithm. Password creation can be done in several iterations.
            var password = new PasswordDeriveBytes(
                PassPhrase,
                saltValueBytes,
                HashAlgorithm,
                PasswordIterations);

            // Use the password to generate pseudo-random bytes for the encryption
            // key. Specify the size of the key in bytes (instead of bits).
            byte[] keyBytes = password.GetBytes(KeySize / 8);

            // Create uninitialized Rijndael encryption object.
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.

            // Generate encryptor from the existing key bytes and initialization 
            // vector. Key size will be defined based on the number of the key 
            // bytes.
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(
                keyBytes,
                initVectorBytes);

            // Define memory stream which will be used to hold encrypted data.
            var memoryStream = new MemoryStream();

            // Define cryptographic stream (always use Write mode for encryption).
            var cryptoStream = new CryptoStream(memoryStream,
                encryptor,
                CryptoStreamMode.Write);

            // Start encrypting.
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);

            // Finish encrypting.
            cryptoStream.FlushFinalBlock();

            // Convert our encrypted data from a memory stream into a byte array.
            byte[] cipherTextBytes = memoryStream.ToArray();

            // Close both streams.
            memoryStream.Close();
            cryptoStream.Close();

            // Convert encrypted data into a base64-encoded string.
            string cipherText = Convert.ToBase64String(cipherTextBytes);

            // Return encrypted string.
            return cipherText;
        }

        /// <summary>
        /// Decrypts a specified string
        /// </summary>
        /// <param name="cipherText">The string to be decrypted</param>
        /// <returns>The decrypted string</returns>
        public static string Decrypt(string cipherText)
        {
            // Convert strings defining encryption key characteristics into byte
            // arrays. Let us assume that strings only contain ASCII codes.
            // If strings include Unicode characters, use Unicode, UTF7, or UTF8
            // encoding.
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(InitVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(SaltValue);

            // Convert our ciphertext into a byte array.
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            // First, we must create a password, from which the key will be 
            // derived. This password will be generated from the specified 
            // passphrase and salt value. The password will be created using
            // the specified hash algorithm. Password creation can be done in
            // several iterations.
            var password = new PasswordDeriveBytes(
                PassPhrase,
                saltValueBytes,
                HashAlgorithm,
                PasswordIterations);

            // Use the password to generate pseudo-random bytes for the encryption
            // key. Specify the size of the key in bytes (instead of bits).
            byte[] keyBytes = password.GetBytes(KeySize / 8);

            // Create uninitialized Rijndael encryption object.
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.

            // Generate decryptor from the existing key bytes and initialization 
            // vector. Key size will be defined based on the number of the key 
            // bytes.
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(
                keyBytes,
                initVectorBytes);

            // Define memory stream which will be used to hold encrypted data.
            var memoryStream = new MemoryStream(cipherTextBytes);

            // Define cryptographic stream (always use Read mode for encryption).
            var cryptoStream = new CryptoStream(memoryStream,
                decryptor,
                CryptoStreamMode.Read);

            // Since at this point we don't know what the size of decrypted data
            // will be, allocate the buffer long enough to hold ciphertext;
            // plaintext is never longer than ciphertext.
            var plainTextBytes = new byte[cipherTextBytes.Length];

            // Start decrypting.
            int decryptedByteCount = cryptoStream.Read(plainTextBytes,
                0,
                plainTextBytes.Length);

            // Close both streams.
            memoryStream.Close();
            cryptoStream.Close();

            // Convert decrypted data into a string. 
            // Let us assume that the original plaintext string was UTF8-encoded.
            string plainText = Encoding.UTF8.GetString(plainTextBytes,
                0,
                decryptedByteCount);

            // Return decrypted string.   
            return plainText;
        }

        /// <summary>
        /// Calculates SHA1 hash
        /// </summary>
        /// <remarks>
        /// <para>
        /// Acceptable Terms are the following 
        /// (case sensitive):
        /// <list>
        /// <item>"MD5"</item>
        /// <item>"SHA1"</item>
        /// <item>"SHA256"</item>
        /// <item>"SHA384"</item>
        /// <item>"SHA512"</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="message">input string</param>
        /// <param name="key">Character encoding</param>
        /// <param name="enc">Hash Algorithm Type</param>
        /// <returns>SHA1 hash</returns>
        public static string ComputeHash(string message, string key, Encoding enc)
        {
            byte[] keyByte = enc.GetBytes(key);

            var hmacsha1 = new HMACSHA1(keyByte);

            byte[] messageBytes = enc.GetBytes(message);
            byte[] hashmessage = hmacsha1.ComputeHash(messageBytes);

            return ByteToString(hashmessage);
        }

        private static string ByteToString(IEnumerable<byte> buff)
        {
            var sbBinary = new StringBuilder();

            foreach (byte b in buff)
            {
                sbBinary.Append(b.ToString("X2")); // hex format
            }

            return sbBinary.ToString();
        }
    }
}
