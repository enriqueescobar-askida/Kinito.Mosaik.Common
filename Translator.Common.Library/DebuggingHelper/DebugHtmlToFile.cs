// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugHtmlToFile.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the DebugHtmlToFile type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.SharePoint;

namespace Translator.Common.Library.DebuggingHelper
{
    public class DebugHtmlToFile : DebugHtml<DebugHtmlToFile>, IDisposable
    {
        private readonly object _lockDebugHtml = new object();
        private readonly string _alphamosaikInstallationPath;

        public DebugHtmlToFile(string html, string alphamosaikInstallationPath)
        {
            _alphamosaikInstallationPath = alphamosaikInstallationPath;
            Init(html);
        }

        public DebugHtmlToFile(StringBuilder html, string alphamosaikInstallationPath)
        {
            _alphamosaikInstallationPath = alphamosaikInstallationPath;
            Init(html.ToString());
        }

        protected override void LogInputHtml(string html, DebugHtmlType type)
        {
            lock (_lockDebugHtml)
            {
                try
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate
                    {
                        string logFilename = String.Format("{0:yyyy-MM-dd-hh-mm-fffffff} ", DateTime.Now) + type + "_" + GetCurrentPageName();
                        string path = Path.Combine(_alphamosaikInstallationPath, @"logs\");

                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        var f7 = new FileInfo(path + logFilename);

                        if (!f7.Exists)
                        {
                            // Create a file to write to.
                            using (f7.CreateText())
                            {
                            }
                        }

                        using (StreamWriter swriterAppend = f7.AppendText())
                        {
                            swriterAppend.WriteLine(html);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Utilities.LogException("Error in DebugHtml.LogInputHtml: " + ex.Message, ex,
                                           EventLogEntryType.Warning);
                }
            }
        }
    }
}