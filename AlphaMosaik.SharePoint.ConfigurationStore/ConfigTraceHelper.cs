namespace AlphaMosaik.SharePoint.ConfigurationStore
{
    using System;
    using System.Diagnostics;

    public class ConfigTraceHelper
    {
        public ConfigTraceHelper(object OwnerClass)
        {
            ownerClassName = OwnerClass.GetType().FullName;
        }

        public ConfigTraceHelper(string OwnerClassName)
        {
            ownerClassName = OwnerClassName;
        }

        private object ownerClassName { get; set; }

        /// <summary>
        /// Write trace output using System.Diagnostics.Trace.
        /// </summary>
        /// <param name="Write">Condition to test before writing - pass trace switch severity.</param>
        /// <param name="Level">Severity level.</param>
        /// <param name="Message">Message to write.</param>
        /// <param name="Args">Arguments to insert into message.</param>
        public void WriteLineIf(bool Write, TraceLevel Level, string Message, params object[] Args)
        {
            Trace.WriteLineIf(Write, string.Format("{0} [{1}] - {2} : {3}",
                                                   DateTime.Now.ToLongTimeString(), Level.ToString().ToUpper(), ownerClassName,
                                                   string.Format(Message, Args)));
        }
    }
}