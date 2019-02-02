// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetInfoTest.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   This is a test class for GetInfoTest and is intended
//   to contain all GetInfoTest Unit Tests
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Alphamosaik.Common.Library.MachineInfo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    /// <summary>
    /// This is a test class for GetInfoTest and is intended
    /// to contain all GetInfoTest Unit Tests
    /// </summary>
    [TestClass]
    public class GetInfoTest
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
        /*
        /// <summary>
        /// A test for GetCpuId
        /// </summary>
        [TestMethod]
        public void GetCpuIdTest()
        {
            const string Expected = "BFEBFBFF000106E5";
            string actual = GetInfo.GetCpuId();
            Assert.AreEqual(Expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
        
        /// <summary>
        /// A test for GetMacAddress
        /// </summary>
        [TestMethod]
        public void GetMacAddressTest()
        {
            const string Expected = "485B39315153";
            string actual = GetInfo.GetMacAddress();
            Assert.AreEqual(Expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
        
        /// <summary>
        /// A test for GetVolumeSerial
        /// </summary>
        [TestMethod]
        public void GetVolumeSerialTest()
        {
            const string StrDriveLetter = "C";
            const string Expected = "A6502408";
            string actual = GetInfo.GetVolumeSerial(StrDriveLetter);
            Assert.AreEqual(Expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }*/
    }
}
