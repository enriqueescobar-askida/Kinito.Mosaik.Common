// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetInfo.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the GetInfo type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Management;

namespace Alphamosaik.Common.Library.MachineInfo
{
    public class GetInfo
    {
        /// <summary>
        /// return Volume Serial Number from hard drive
        /// </summary>
        /// <param name="strDriveLetter">[optional] Drive letter</param>
        /// <returns>[string] VolumeSerialNumber</returns>
        public static string GetVolumeSerial(string strDriveLetter)
        {
            try
            {
                if (string.IsNullOrEmpty(strDriveLetter))
                    strDriveLetter = "C";

                using (var disk = new ManagementObject("win32_logicaldisk.deviceid=\"" + strDriveLetter + ":\""))
                {
                    disk.Get();
                    return disk["VolumeSerialNumber"].ToString();
                }
            }
            catch (Exception)
            {
                return "A6502408";
            }
        }

        /// <summary>
        /// Returns MAC Address from first Network Card in Computer
        /// </summary>
        /// <returns>[string] MAC Address</returns>
        public static string GetMacAddress()
        {
            try
            {
                ManagementObjectCollection moc;
                using (var mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
                {
                    moc = mc.GetInstances();
                }

                string macAddress = string.Empty;
                foreach (ManagementObject mo in moc)
                {
                    if (macAddress == string.Empty)
                    {
                        // only return MAC Address from first card
                        if ((bool)mo["IPEnabled"])
                            macAddress = mo["MacAddress"].ToString();
                    }

                    mo.Dispose();
                }

                macAddress = macAddress.Replace(":", string.Empty);
                return macAddress;
            }
            catch (Exception)
            {
                return "485B39315153";
            }
        }

        /// <summary>
        /// Return processorId from first CPU in machine
        /// </summary>
        /// <returns>[string] ProcessorId</returns>
        public static string GetCpuId()
        {
            try
            {
                string cpuInfo = string.Empty;
                ManagementObjectCollection moc;
                using (var mc = new ManagementClass("Win32_Processor"))
                {
                    moc = mc.GetInstances();
                }

                foreach (ManagementObject mo in moc)
                {
                    if (cpuInfo == string.Empty)
                    {
                        // only return cpuInfo from first CPU
                        cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                    }
                }

                return cpuInfo;
            }
            catch (Exception ex)
            {
                return "BFEBFBFF000106E5";
            }
        }
    }
}
