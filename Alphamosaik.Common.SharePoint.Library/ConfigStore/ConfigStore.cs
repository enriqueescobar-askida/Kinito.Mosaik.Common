// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigStore.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Utility class for retrieving configuration values within a SharePoint site. Where multiple values need to
//   be retrieved, the GetMultipleValues() method should be used where possible.
//   Created by Chris O'Brien (www.sharepointnutsandbolts.com).
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics;

namespace Alphamosaik.Common.SharePoint.Library.ConfigStore
{
    /// <summary>
    /// Utility class for retrieving configuration values within a SharePoint site. Where multiple values need to
    /// be retrieved, the GetMultipleValues() method should be used where possible.
    /// Created by Chris O'Brien (www.sharepointnutsandbolts.com).
    /// </summary>
    public class ConfigStore : Store<ConfigStore>
    {
        protected ConfigStore()
        {
#if DEBUG
            _traceSwitch.Level = TraceLevel.Verbose;
#endif
        }

        protected override string DefaultListName
        {
            get
            {
                return "Configuration Store";
            }
        }
    }
}
