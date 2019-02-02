/* Program : WSPBuilder
 * Created by: Carsten Keutmann
 * Date : 2009 Juli
 *  
 * The WSPBuilder comes under GNU GENERAL PUBLIC LICENSE (GPL).
 */
using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace Translator2009.Installer
{
    public class WindowsServices
    {
        private const string SPTIMERV4_NAME = "SPTimerV4";
        private const string SPADMINV4_NAME = "SPAdminV4";

        private string _spTimerName = null;
        public string SPTimerName
        {
            get 
            {
                if (string.IsNullOrEmpty(_spTimerName))
                {
                    _spTimerName = SPTIMERV4_NAME;
                }
                return _spTimerName; 
            }
        }

        private string _spAdminName = null;
        public string SPAdminName
        {
            get
            {
                if (String.IsNullOrEmpty(_spAdminName))
                {
                    _spAdminName = SPADMINV4_NAME;
                }
                return _spAdminName;
            }
        }

        private WindowsServices()
        {
        }

        public static void Stop(ServiceController sc)
        {
            
            string name = typeof(WindowsServices).Name;

            TimeSpan timeout = new TimeSpan(0, 0, 30);

            //check the status of the service
            if (sc.Status == ServiceControllerStatus.Running)
            {
                //make sure the service is stoppable
                if (sc.CanStop)
                {
                    Trace.TraceInformation("Restarting the " + sc.DisplayName + " service.");

                    //stop SPTimerV3
                    sc.Stop();

                    try
                    {
                        //wait 30 seconds for SPTImerV3 to stop
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                        Trace.TraceInformation(sc.DisplayName + " service stopped successfully.");
                    }
                    catch (System.ServiceProcess.TimeoutException)
                    {
                        Trace.TraceError(sc.DisplayName + " service did not respond to the stop command in a timely fashion.");
                    }
                }
                else
                {
                    Trace.TraceError(sc.DisplayName + " service cannot be stopped.");
                }
            }
        }

        public static void Start(ServiceController sc)
        {
            TimeSpan timeout = new TimeSpan(0, 0, 30);

            string name = typeof(WindowsServices).Name;
         
            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                //start SPTimerV3
                sc.Start();

                try
                {
                    //wait 30 seconds for SPTimerV3 to start
                    sc.WaitForStatus(ServiceControllerStatus.Running, timeout);

                    Trace.TraceInformation(sc.DisplayName + " service started successfully.");
                }
                catch (System.ServiceProcess.TimeoutException)
                {
                    Trace.TraceError(sc.DisplayName + " service did not respond to the start command in a timely fashion.");
                }
            }
        }

        public static void Restart(string serviceName)
        {
            //create instance of service controller
            ServiceController sc = new ServiceController(serviceName);
            try
            {
                Stop(sc);
                Start(sc);
            }
            finally
            {
                //cleanup
                sc.Close();
                sc = null;
            }
        }


        #region Singleton

        public static WindowsServices Current
        {
            get
            {
                return Nested.current;
            }
        }

        class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly WindowsServices current = new WindowsServices();
        }

        #endregion
    }
}
