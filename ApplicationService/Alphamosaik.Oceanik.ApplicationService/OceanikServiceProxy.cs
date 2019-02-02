// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OceanikServiceProxy.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Oceanik.ApplicationService
{
    using System;

    using Microsoft.SharePoint.Administration;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class OceanikServiceProxy : SPIisWebServiceProxy, IServiceProxyAdministration
    {
        public OceanikServiceProxy()
        {
        }

        public OceanikServiceProxy(SPFarm farm)
            : base(farm)
        {
        }

        public SPServiceApplicationProxy CreateProxy(Type serviceApplicationProxyType, string name, Uri serviceApplicationUri, SPServiceProvisioningContext provisioningContext)
        {
            if (serviceApplicationProxyType != typeof(OceanikServiceApplicationProxy))
                throw new NotSupportedException();

            return new OceanikServiceApplicationProxy(name, this, serviceApplicationUri);
        }

        public SPPersistedTypeDescription GetProxyTypeDescription(Type serviceApplicationProxyType)
        {
            return new SPPersistedTypeDescription("Wingtip Calculator Service Proxy", 
                    "Custom service application proxy providing simple calculation capabilities.");
        }

        public Type[] GetProxyTypes()
        {
            return new[] { typeof(OceanikServiceApplicationProxy) };
        }
    }
}
