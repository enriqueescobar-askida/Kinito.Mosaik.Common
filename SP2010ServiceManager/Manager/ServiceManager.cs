// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceManager.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the ServiceManager type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceProcess;

namespace SP2010ServiceManager.Manager
{
    public class ServiceManager
    {
        /// <summary>
        /// List of services. The service order is important as the services are stopped and started using the order of the list.
        /// </summary>
        private static readonly List<string> ServiceList = new List<string>(new[]
                                                                {
                                                                    "MSSQLSERVER", "SQLWriter", "SQLSERVERAGENT", "SQLBrowser", "msftesql", "MSSQL$SHAREPOINT",
                                                                    "W3SVC", 
                                                                    "SPTimerV4", "DCLoadBalancer14", "DCLauncher14", "SPWriterV4",
                                                                    "SPTraceV4", "SPAdminV4", "WebAnalyticsService",
                                                                    "OSearch14", "SPSearch4", "SPUserCodeV4"
                                                                });

        private readonly BackgroundWorker _backgroundWorker;

        public ServiceManager(BackgroundWorker backgroundWorker)
        {
            _backgroundWorker = backgroundWorker;
        }

        /// <summary>
        /// Starts all the services.
        /// </summary>
        /// <returns>
        /// The start services.
        /// </returns>
        public bool StartServices()
        {
            try
            {
                var startedSuccesfully = true;
                var sharePointServices = LoadSharePointServices();
                foreach (var service in sharePointServices)
                {
                    startedSuccesfully = startedSuccesfully && StartService(service);
                }

                ReleaseServices(sharePointServices);
                return startedSuccesfully;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Stops all the services.
        /// </summary>
        /// <returns>
        /// The stop services.
        /// </returns>
        public bool StopServices()
        {
            try
            {
                var stoppedSuccesfully = true;
                var sharePointServices = LoadSharePointServices();

                // Go through the list in reverse order
                for (var index = sharePointServices.Count - 1; index >= 0; index--)
                {
                    var service = sharePointServices[index];
                    stoppedSuccesfully = stoppedSuccesfully && StopService(service);
                }

                ReleaseServices(sharePointServices);
                return stoppedSuccesfully;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a list of all the services along with their current status.
        /// </summary>
        /// <returns>return the list of windows services</returns>
        public List<WindowsService> GetSharePointServices()
        {
            var services = new List<WindowsService>();

            var sharePointServices = LoadSharePointServices();
            foreach (var service in sharePointServices)
            {
                service.Refresh();
                services.Add(new WindowsService(service));
            }

            ReleaseServices(sharePointServices);

            return services;
        }

        /// <summary>
        /// Sets all the sharepoint services to startup manually.
        /// </summary>
        public void SetToManualStartup()
        {
            foreach (var serviceCode in ServiceList)
            {
                if (!serviceCode.Equals("W3SVC", StringComparison.InvariantCultureIgnoreCase))
                    SetServiceTypeManual(serviceCode);
            }
        }

        /// <summary>
        /// Checks if all the services are set to startup manually.
        /// </summary>
        /// <returns>True is all the services are set to startup manually.</returns>
        public bool IsServicesSetToManualStartup()
        {
            foreach (var serviceCode in ServiceList)
            {
                // SQLWriter is set to Automatic by SQL Server even if we change the startup to manual, let's ignore it.
                var service = new ServiceObject(serviceCode);
                if (!serviceCode.Equals("SQLWriter", StringComparison.InvariantCultureIgnoreCase) && !service.IsStartupManual())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Clears the memory used by the services.
        /// </summary>
        /// <param name="serviceControllers">
        /// The service Controllers.
        /// </param>
        private static void ReleaseServices(IEnumerable<ServiceController> serviceControllers)
        {
            try
            {
                foreach (var service in serviceControllers)
                {
                    service.Close();
                    service.Dispose();
                }

                GC.Collect();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Loads all the services.
        /// </summary>
        /// <returns> List of ServiceController</returns>
        private static List<ServiceController> LoadSharePointServices()
        {
            return (from serviceCode in ServiceList
                    let service = GetService(serviceCode)
                    where service != null
                    select new ServiceController(serviceCode)).ToList();
        }

        /// <summary>
        /// Tries to load the service specified by the serviceCode.
        /// </summary>
        /// <param name="serviceCode">The name of the service</param>
        /// <returns>Returns null if it fails.</returns>
        private static ServiceController GetService(string serviceCode)
        {
            var service = new ServiceController(serviceCode);

            try
            {
                var name = service.ServiceName;
            }
            catch (Exception)
            {
                return null;
            }

            return service;
        }

        /// <summary>
        /// Sets the specified service to startup manually.
        /// </summary>
        /// <param name="serviceCode">The name of the service</param>
        private static void SetServiceTypeManual(string serviceCode)
        {
            try
            {
                var service = new ServiceObject(serviceCode);

                if (!service.IsStartupDisabled())
                    service.SetStartupManual();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Starts the specified service.
        /// </summary>
        /// <param name="service"> The service to start
        /// </param>
        /// <returns>
        /// The start service.
        /// </returns>
        private bool StartService(ServiceController service)
        {
            try
            {
                service.Refresh();

                var serviceObj = new ServiceObject(service.ServiceName);

                if (service.Status == ServiceControllerStatus.Stopped && !serviceObj.IsStartupDisabled())
                {
                    ReportProgress(string.Format("Starting {0}...\r\n", service.DisplayName));
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Stops the specified service.
        /// </summary>
        /// <param name="service"> The service to stop
        /// </param>
        /// <returns>
        /// The stop service.
        /// </returns>
        private bool StopService(ServiceController service)
        {
            try
            {
                service.Refresh();
                if (service.Status == ServiceControllerStatus.Running)
                {
                    ReportProgress(string.Format("Stopping {0}...\r\n", service.DisplayName));
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ReportProgress(string message)
        {
            _backgroundWorker.ReportProgress(0, message);
        }
    }
}