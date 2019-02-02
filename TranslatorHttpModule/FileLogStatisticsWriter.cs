// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileLogStatisticsWriter.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the FileLogStatisticsWriter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Alphamosaik.Common.Library.Statistics;
using Microsoft.SharePoint;

namespace TranslatorHttpHandler
{
    public class FileLogStatisticsWriter : IStatisticsWriter
    {
        private static readonly object LockObject = new object();
        private readonly string _logFilename;
        private readonly StringBuilder _buffer = new StringBuilder();

        public FileLogStatisticsWriter(string logFilename)
        {
            _logFilename = logFilename;
        }

        public void Write(StatisticsEntry statistics)
        {
            lock (LockObject)
            {
                try
                {
                    _buffer.AppendLine(statistics.Title + " : " + statistics.Message);
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry("FileLogStatisticsWriter", ex.Message + Environment.NewLine + ex, EventLogEntryType.Warning);
                }
            }
        }

        public void Flush()
        {
            lock (LockObject)
            {
                try
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate
                    {
                        string path = Path.GetDirectoryName(_logFilename);

                        if (path != null && !Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        var f7 = new FileInfo(_logFilename);

                        if (!f7.Exists)
                        {
                            // Create a file to write to.
                            using (f7.CreateText())
                            {
                            }
                        }

                        using (StreamWriter swriterAppend = f7.AppendText())
                        {
                            swriterAppend.WriteLine(_buffer.ToString());
                        }
                    });
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry("FileLogStatisticsWriter", ex.Message + Environment.NewLine + ex, EventLogEntryType.Warning);
                }
            }
        }
    }
}