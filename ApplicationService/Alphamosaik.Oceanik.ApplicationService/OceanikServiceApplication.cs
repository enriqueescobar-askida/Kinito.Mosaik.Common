// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OceanikServiceApplication.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Oceanik.ApplicationService
{
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Utilities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class OceanikServiceApplication : SPIisWebServiceApplication, IOceanikContract
    {
        public OceanikServiceApplication()
        {
        }

        private OceanikServiceApplication(string name, OceanikService service, SPIisWebServiceApplicationPool appPool)
            : base(name, service, appPool)
        {
        }

        public override string TypeName
        {
            get { return "Oceanik Service Application"; }
        }

        public override SPAdministrationLink ManageLink
        {
            get { return new SPAdministrationLink("/_admin/OceanikApplicationService/Manage.aspx"); }
        }

        public override SPAdministrationLink PropertiesLink
        {
            get { return new SPAdministrationLink("/_admin/OceanikApplicationService/Manage.aspx"); }
        }

        protected override string InstallPath
        {
            get { return SPUtility.GetGenericSetupPath(@"WebServices\Oceanik"); }
        }

        protected override string VirtualPath
        {
            get { return "OceanikService.svc"; }
        }

        public static OceanikServiceApplication Create(string name, OceanikService service, SPIisWebServiceApplicationPool appPool)
        {
            // ... validation code omitted ...

            // create the service application
            var serviceApplication = new OceanikServiceApplication(name, service, appPool);
            serviceApplication.Update();

            // register the supported endpoints
            serviceApplication.AddServiceEndpoint("http", SPIisWebServiceBindingType.Http);
            serviceApplication.AddServiceEndpoint("https", SPIisWebServiceBindingType.Https, "secure");

            return serviceApplication;
        }
    }
}
