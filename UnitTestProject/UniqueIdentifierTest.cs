// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UniqueIdentifierTest.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   This is a test class for UniqueIdentifierTest and is intended
//   to contain all UniqueIdentifierTest Unit Tests
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using Alphamosaik.Common.Library;
using Alphamosaik.Common.Library.Licensing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    /// <summary>
    /// This is a test class for UniqueIdentifierTest and is intended
    /// to contain all UniqueIdentifierTest Unit Tests
    /// </summary>
    [TestClass()]
    public class UniqueIdentifierTest
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        // You can use the following additional attributes as you write your tests:
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext)
        // {
        // }
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup()
        // {
        // }
        // Use TestInitialize to run code before running each test
        // [TestInitialize()]
        // public void MyTestInitialize()
        // {
        // }
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup()
        // {
        // }
        #endregion

        public string ConvertToHex(string asciiString)
        {
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(asciiString);
            return Convert.ToBase64String(encbuff);
        }

        /// <summary>
        /// A test for GetIdentifier
        /// </summary>
        [TestMethod()]
        public void GetIdentifierTest()
        {
            const string Expected = "485B39315153-BFEBFBFF000106E5-A6502408";

            string actualEncryptedHex = PrivateKey.GetPrivateKey();

            var license = new License
                              {
                                  Type = License.LicenseType.Trial,
                                  TrialStart = new DateTime(2010, 11, 01),
                                  TrialEnd = new DateTime(2010, 11, 09)
                              };

            string licenseString = license.GenerateLicense(actualEncryptedHex);
            WriteLicenseKey(licenseString);

            string fullPath = Path.Combine(@"C:\Program Files\Alphamosaik\SharepointTranslator2010", "license.dat");

            if (File.Exists(fullPath))
            {
                string license1 = File.ReadAllText(fullPath);

                var license2 = new License(actualEncryptedHex, license1);

                if (license2.IsValide)
                {
                    string words1 = LoremIpsumGenerator.GetWords(512);
                }
            }

            string words = LoremIpsumGenerator.GetWords(512);

            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        public void WriteLicenseKey(string licenceKey)
        {
            try
            {
                string fullPath = Path.Combine(@"C:\Program Files\Alphamosaik\SharepointTranslator2010", "license.dat");

                if (!Directory.Exists(@"C:\Program Files\Alphamosaik\SharepointTranslator2010"))
                {
                    Directory.CreateDirectory(@"C:\Program Files\Alphamosaik\SharepointTranslator2010");
                }

                var f7 = new FileInfo(fullPath);

                if (!f7.Exists)
                {
                    // Create a file to write to.
                    using (f7.CreateText())
                    {
                    }

                    using (StreamWriter swriterAppend = f7.AppendText())
                    {
                        swriterAppend.WriteLine(licenceKey);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

// 434B73377A784348424A4E7637632B585164694D4C454D6A53772B77746339446B61754D76345168744B48474F75594D344A3935596B34633341506B46707163
// 485B39315153-BFEBFBFF000106E5-A6502408