// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OceanikServiceClient.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Oceanik.ApplicationService
{
    using Microsoft.SharePoint;

    public class OceanikServiceClient
    {
        private SPServiceContext _serviceContext;

        public OceanikServiceClient(SPServiceContext serviceContext)
        {
            _serviceContext = serviceContext;
        }
    }
}
