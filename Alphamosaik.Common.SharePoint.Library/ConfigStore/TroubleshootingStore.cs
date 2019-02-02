// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TroubleshootingStore.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the TroubleshootingStore type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics;

namespace Alphamosaik.Common.SharePoint.Library.ConfigStore
{
    public class TroubleshootingStore : Store<TroubleshootingStore>
    {
        protected TroubleshootingStore()
        {
#if DEBUG
            _traceSwitch.Level = TraceLevel.Verbose;
#endif
        }

        protected override string DefaultListName
        {
            get
            {
                return "Troubleshooting Store";
            }
        }
    }
}