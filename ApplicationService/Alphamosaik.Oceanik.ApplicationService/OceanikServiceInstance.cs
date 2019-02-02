// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OceanikServiceInstance.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Oceanik.ApplicationService
{
    using Microsoft.SharePoint.Administration;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class OceanikServiceInstance : SPIisWebServiceInstance
    {
        public OceanikServiceInstance()
        {
        }

        public OceanikServiceInstance(SPServer server, OceanikService service)
            : base(server, service)
        {
        }

        public override string DisplayName
        {
            get { return "Oceanik Service"; }
        }

        public override string Description
        {
            get { return "Oceanik providing simple translation services."; }
        }

        public override string TypeName
        {
            get { return "Oceanik Service"; }
        }
    }
}
