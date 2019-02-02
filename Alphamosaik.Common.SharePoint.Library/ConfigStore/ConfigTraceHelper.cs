// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigTraceHelper.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the ConfigTraceHelper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;

namespace Alphamosaik.Common.SharePoint.Library.ConfigStore
{
    public class ConfigTraceHelper
    {
        public ConfigTraceHelper(object ownerClass)
        {
            OwnerClassName = ownerClass.GetType().FullName;
        }

        public ConfigTraceHelper(string ownerClassName)
        {
            OwnerClassName = ownerClassName;
        }

        private object OwnerClassName { get; set; }

        /// <summary>
        /// Write trace output using System.Diagnostics.Trace.
        /// </summary>
        /// <param name="write">Condition to test before writing - pass trace switch severity.</param>
        /// <param name="level">Severity level.</param>
        /// <param name="message">Message to write.</param>
        /// <param name="args">Arguments to insert into message.</param>
        public void WriteLineIf(bool write, TraceLevel level, string message, params object[] args)
        {
            Trace.WriteLineIf(write, string.Format("{0} [{1}] - {2} : {3}",
                                                   DateTime.Now.ToLongTimeString(), level.ToString().ToUpper(), OwnerClassName,
                                                   string.Format(message, args)));
        }
    }
}