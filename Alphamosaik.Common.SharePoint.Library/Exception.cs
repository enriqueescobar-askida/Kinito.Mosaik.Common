using System;
using System.Diagnostics;
using Microsoft.SharePoint;

namespace Alphamosaik.Common.SharePoint.Library
{
    public class Exception
    {
        public static void LogException(string source, string eventMessage)
        {
            string path = Utilities.GetHttpContextPath();

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                if (EventLog.SourceExists(source))
                {
                    EventLog.WriteEntry(source,
                                        eventMessage + Environment.NewLine +
                                        path,
                                        EventLogEntryType.Information);
                }
            });

            Trace.WriteLine(source + " :" + eventMessage + Environment.NewLine + path);
        }

        public static void LogException(string source, string eventMessage, EventLogEntryType type)
        {
            string path = Utilities.GetHttpContextPath();

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                if (EventLog.SourceExists(source))
                {
                    EventLog.WriteEntry(source,
                                        eventMessage + Environment.NewLine +
                                        path, type);
                }
            });

            Trace.WriteLine(source + " :" + eventMessage + Environment.NewLine + path);
        }

        public static void LogException(string source, string eventMessage, System.Exception e, EventLogEntryType type)
        {
            string path = Utilities.GetHttpContextPath();

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                if (EventLog.SourceExists(source))
                {
                    EventLog.WriteEntry(source,
                                        eventMessage + Environment.NewLine + e +
                                        Environment.NewLine + path, type);
                }
            });

            Trace.WriteLine(source + " :" + eventMessage + Environment.NewLine + path);
        }
    }
}
