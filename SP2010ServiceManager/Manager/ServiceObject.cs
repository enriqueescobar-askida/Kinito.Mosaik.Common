// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceObject.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   The ServiceController class in .NET does not provide support for quering and changing the Startup type.
//   We use WMI Management Objects to do this.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Management;

namespace SP2010ServiceManager.Manager
{
    /// <summary>
    /// The ServiceController class in .NET does not provide support for quering and changing the Startup type.
    /// We use WMI Management Objects to do this.
    /// </summary>
    internal class ServiceObject
    {
        private readonly ManagementObject _managementObject;

        public ServiceObject(string serviceCode)
        {
            _managementObject = GetManagementObject(serviceCode);
        }

        public void SetStartupManual()
        {
            _managementObject.InvokeMethod("ChangeStartMode", new object[] { "Manual" });
        }

        public bool IsStartupManual()
        {
            return _managementObject["StartMode"].ToString().Equals("manual", StringComparison.InvariantCultureIgnoreCase);
        }

        public bool IsStartupDisabled()
        {
            return _managementObject["StartMode"].ToString().Equals("Disabled", StringComparison.InvariantCultureIgnoreCase);
        }

        private static ManagementObject GetManagementObject(string serviceCode)
        {
            return new ManagementObject(new ManagementPath(string.Format("Win32_Service.Name='{0}'", serviceCode)));
        }
    }
}