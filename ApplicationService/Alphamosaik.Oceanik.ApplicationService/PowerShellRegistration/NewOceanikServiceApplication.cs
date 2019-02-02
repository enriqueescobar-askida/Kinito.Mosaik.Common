// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewOceanikServiceApplication.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the NewOceanikServiceApplication type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Oceanik.ApplicationService.PowerShellRegistration
{
    using System;
    using System.Management.Automation;

    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.PowerShell;

    [Cmdlet(VerbsCommon.New, "OceanikServiceApplication", SupportsShouldProcess = true)]
    public class NewOceanikServiceApplication : SPCmdlet
    {
        #region cmdlet parameters
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Name;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public SPIisWebServiceApplicationPoolPipeBind ApplicationPool;
        #endregion

        protected override bool RequireUserFarmAdmin()
        {
            return true;
        }

        protected override void InternalProcessRecord()
        {
            #region validation stuff
            // ensure can hit farm
            SPFarm farm = SPFarm.Local;
            if (farm == null)
            {
                ThrowTerminatingError(new InvalidOperationException("SharePoint farm not found."), ErrorCategory.ResourceUnavailable, this);
                SkipProcessCurrentRecord();
            }

            // ensure can hit local server
            SPServer server = SPServer.Local;
            if (server == null)
            {
                ThrowTerminatingError(new InvalidOperationException("SharePoint local server not found."), ErrorCategory.ResourceUnavailable, this);
                SkipProcessCurrentRecord();
            }

            // ensure can hit service application
            var service = farm.Services.GetValue<OceanikService>();
            if (service == null)
            {
                ThrowTerminatingError(new InvalidOperationException("Wingtip Calc Service not found (likely not installed)."), ErrorCategory.ResourceUnavailable, this);
                SkipProcessCurrentRecord();
            }

            // ensure can hit app pool
            SPIisWebServiceApplicationPool appPool = this.ApplicationPool.Read();
            if (appPool == null)
            {
                ThrowTerminatingError(new InvalidOperationException("Application pool not found."), ErrorCategory.ResourceUnavailable, this);
                SkipProcessCurrentRecord();
            }
            #endregion

            // verify a service app doesn't already exist
            var existingServiceApp = service.Applications.GetValue<OceanikServiceApplication>();
            if (existingServiceApp != null)
            {
                WriteError(new InvalidOperationException("Wingtip Calc Service Application already exists."),
                    ErrorCategory.ResourceExists,
                    existingServiceApp);
                SkipProcessCurrentRecord();
            }

            // create & provision the service app
            if (ShouldProcess(this.Name))
            {
                OceanikServiceApplication serviceApp = OceanikServiceApplication.Create(
                    this.Name,
                    service,
                    appPool);

                // provision the service app
                serviceApp.Provision();

                // pass service app back to the PowerShell
                WriteObject(serviceApp);
            }
        }
    }
}
