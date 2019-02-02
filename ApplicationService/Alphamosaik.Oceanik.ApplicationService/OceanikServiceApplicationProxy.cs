// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OceanikServiceApplicationProxy.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Oceanik.ApplicationService
{
    using System;
    using System.ServiceModel;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;

    public class OceanikServiceApplicationProxy : SPIisWebServiceApplicationProxy
    {
        private ChannelFactory<IOceanikContract> _channelFactory;
        private object _channelFactoryLock = new object();
        private string _endpointConfigName;

        [Persisted]
        private SPServiceLoadBalancer _loadBalancer;

        public OceanikServiceApplicationProxy()
        {
        }

        public OceanikServiceApplicationProxy(string name, OceanikServiceProxy proxy, Uri serviceAddress)
            : base(name, proxy, serviceAddress)
        {
            // create instance of a new load balancer
            _loadBalancer = new SPRoundRobinServiceLoadBalancer(serviceAddress);
        }

        public override string TypeName
        {
            get { return "Wingtip Calculator Service Application"; }
        }
    }
}
