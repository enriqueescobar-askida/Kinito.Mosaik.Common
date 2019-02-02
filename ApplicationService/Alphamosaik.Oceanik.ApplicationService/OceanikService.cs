// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OceanikService.cs" company="AlphaMosaik">
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

    public class OceanikService : SPIisWebService, IServiceAdministration
    {
        public OceanikService()
        {
        }

        public OceanikService(SPFarm farm)
            : base(farm)
        {
        }

        #region Implementation of IServiceAdministration

        public Type[] GetApplicationTypes()
        {
            throw new NotImplementedException();
        }

        public SPPersistedTypeDescription GetApplicationTypeDescription(Type serviceApplicationType)
        {
            throw new NotImplementedException();
        }

        public SPServiceApplication CreateApplication(string name, Type serviceApplicationType, SPServiceProvisioningContext provisioningContext)
        {
            throw new NotImplementedException();
        }

        public SPServiceApplicationProxy CreateProxy(string name, SPServiceApplication serviceApplication, SPServiceProvisioningContext provisioningContext)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
