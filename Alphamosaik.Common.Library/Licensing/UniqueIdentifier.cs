// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UniqueIdentifier.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the UniqueIdentifier type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text;

namespace Alphamosaik.Common.Library.Licensing
{
    public class UniqueIdentifier
    {
        public static string GetIdentifier()
        {
            var result = new StringBuilder();

            result.AppendFormat("{0}-{1}-{2}", MachineInfo.GetInfo.GetMacAddress(), MachineInfo.GetInfo.GetCpuId(), MachineInfo.GetInfo.GetVolumeSerial(string.Empty));

            return result.ToString();
        }
    }
}
