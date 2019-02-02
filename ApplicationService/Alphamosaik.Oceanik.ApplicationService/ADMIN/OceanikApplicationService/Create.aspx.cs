// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Create.aspx.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the Create type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Oceanik.ApplicationService.ADMIN.OceanikApplicationService
{
    using System;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.WebControls;

    public partial class Create : LayoutsPageBase
    {
        // page web controls
        protected IisWebServiceApplicationPoolSection ApplicationPoolSelection;
        protected InputFormTextBox ServiceAppName;
        protected InputFormCheckBox DefaultServiceApp;

        // wire up event on the OK button
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // get reference to OK button on dialog master page & wire up handler to it's OK button
            ((DialogMaster)this.Page.Master).OkButton.Click += this.OkButton_Click;
        }

        void OkButton_Click(object sender, EventArgs e)
        {
            // create the service app
            SetupOceanikServiceApp();

            // finish call
            SendResponseForPopUI();
        }

        private void SetupOceanikServiceApp()
        {
            // create a long running op..
            using (var op = new SPLongOperation(this))
            {
                op.Begin();

                try
                {
                    // get reference to the installed service
                    var service = SPFarm.Local.Services.GetValue<OceanikService>();

                    // create the service application
                    OceanikServiceApplication serviceApp = CreateServiceApplication(service);

                    // if the service instance isn't running, start it up
                    StartServiceInstances();

                    // create service app proxy
                    CreateServiceApplicationProxy(serviceApp);
                }
                catch (Exception e)
                {
                    throw new SPException("Error creating Oceanik service application.", e);
                }
            }
        }

        private OceanikServiceApplication CreateServiceApplication(OceanikService service)
        {
            // create service app
            OceanikServiceApplication serviceApp = OceanikServiceApplication.Create(
                ServiceAppName.Text,
                service,
                ApplicationPoolSelection.GetOrCreateApplicationPool());
            serviceApp.Update();

            // start it if it isn't already started
            serviceApp.Status = SPObjectStatus.Online;

            // configure service app endpoint
            serviceApp.AddServiceEndpoint(string.Empty, SPIisWebServiceBindingType.Http);
            serviceApp.Update(true);

            // now provision the service app
            serviceApp.Provision();
            return serviceApp;
        }

        private void CreateServiceApplicationProxy(OceanikServiceApplication serviceApp)
        {
            // get reference to the installed service proxy
            OceanikServiceProxy serviceProxy = SPFarm.Local.ServiceProxies.GetValue<OceanikServiceProxy>();

            // create service app proxy
            OceanikServiceApplicationProxy serviceAppProxy = new OceanikServiceApplicationProxy(
                ServiceAppName.Text + " Proxy",
                serviceProxy,
                serviceApp.Uri);
            serviceAppProxy.Update(true);

            // provision service app proxy
            serviceAppProxy.Provision();

            // start it if it isn't already started
            if (serviceAppProxy.Status != SPObjectStatus.Online)
                serviceAppProxy.Status = SPObjectStatus.Online;
            serviceAppProxy.Update(true);

            // add the proxy to the default group if selected
            if (DefaultServiceApp.Checked)
            {
                SPServiceApplicationProxyGroup defaultGroup = SPServiceApplicationProxyGroup.Default;
                defaultGroup.Add(serviceAppProxy);
                defaultGroup.Update(true);
            }
        }

        private static void StartServiceInstances()
        {
            // loop through all service instances on the current server and see if the one for
            //      this service app is running/not
            foreach (SPServiceInstance serviceInstance in SPServer.Local.ServiceInstances)
            {
                OceanikServiceInstance calcServiceInstance = serviceInstance as OceanikServiceInstance;

                // if this one isn't running, start it up
                if ((calcServiceInstance != null) &&
                        (calcServiceInstance.Status != SPObjectStatus.Online))
                    calcServiceInstance.Status = SPObjectStatus.Online;
            }
        }

        private Guid ApplicationId
        {
            get
            {
                if (base.Request.QueryString != null)
                {
                    string s = base.Request.QueryString["appid"];
                    if (string.IsNullOrEmpty(s))
                        return Guid.Empty;

                    try
                    {
                        return new Guid(s);
                    }
                    catch (FormatException)
                    {
                        throw new Exception();
                    }
                }
                return Guid.Empty;
            }
        }
    }
}
