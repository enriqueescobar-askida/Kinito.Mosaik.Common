// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OceanikServiceHostFactory.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the OceanikServiceHostFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Oceanik.ApplicationService
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activation;

    using Microsoft.SharePoint;

    internal sealed class OceanikServiceHostFactory : ServiceHostFactory
    {
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            var serviceHost = new ServiceHost(typeof(OceanikServiceApplication), baseAddresses);

            // configure the service for claims
            serviceHost.Configure(SPServiceAuthenticationMode.Claims);

            return serviceHost;
        }
    }
}
