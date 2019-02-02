// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OceanikApplicationServiceInstaller.EventReceiver.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Runtime.InteropServices;

using Microsoft.SharePoint;

namespace Alphamosaik.Oceanik.ApplicationService.Features.OceanikApplicationServiceInstaller
{
    using Microsoft.SharePoint.Administration;

    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>
    [Guid("98f747e8-3f68-4315-b944-40b18b65c3a9")]
    public class OceanikApplicationServiceInstallerEventReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            // install the service
            var service = SPFarm.Local.Services.GetValue<OceanikService>();
            if (service == null)
            {
                service = new OceanikService(SPFarm.Local);
                service.Update();
            }

            // install the service proxy
            var serviceProxy = SPFarm.Local.ServiceProxies.GetValue<OceanikServiceProxy>();
            if (serviceProxy == null)
            {
                serviceProxy = new OceanikServiceProxy(SPFarm.Local);
                serviceProxy.Update(true);
            }

            // with service added to the farm, install instance
            var serviceInstance = new OceanikServiceInstance(SPServer.Local, service);
            serviceInstance.Update(true);
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            // uninstall the instance
            var serviceInstance =
                 SPFarm.Local.Services.GetValue<OceanikServiceInstance>();
            if (serviceInstance != null)
                SPServer.Local.ServiceInstances.Remove(serviceInstance.Id);

            // uninstall the service proxy
            var serviceProxy = SPFarm.Local.ServiceProxies.GetValue<OceanikServiceProxy>();
            if (serviceProxy != null)
            {
                SPFarm.Local.ServiceProxies.Remove(serviceProxy.Id);
            }

            // uninstall the service
            var service = SPFarm.Local.Services.GetValue<OceanikService>();
            if (service != null)
                SPFarm.Local.Services.Remove(service.Id);
        }
    }
}
