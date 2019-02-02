// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowsService.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the ServiceStatus type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.ServiceProcess;

namespace SP2010ServiceManager.Manager
{
    public enum ServiceStatus
    {
        Stopped,
        Running,
        Paused,
        Unknown
    }

    public class WindowsService
    {
        public WindowsService(ServiceController serviceController)
        {
            Name = serviceController.DisplayName;
            switch (serviceController.Status)
            {
                case ServiceControllerStatus.Running:
                    Status = ServiceStatus.Running;
                    break;
                case ServiceControllerStatus.Stopped:
                    Status = ServiceStatus.Stopped;
                    break;
                case ServiceControllerStatus.Paused:
                    Status = ServiceStatus.Paused;
                    break;
                default:
                    Status = ServiceStatus.Unknown;
                    break;
            }
        }

        public string Name { get; set; }

        public ServiceStatus Status { get; set; }
    }
}
