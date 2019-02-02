using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace AlphaMosaik.Logger.Administration
{
    public class AdministrationFeatureReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPWebService.AdministrationService.ApplyApplicationContentToLocalServer();
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            return;
        }

        public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        {
            return;
        }

        public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        {
            return;
        }
    }
}
