// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PerformanceEntry.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Represents a log message.  Contains the common properties that are required for all log messages.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace Alphamosaik.Common.Library.Performance
{
    /// <summary>
    /// Represents a log message.  Contains the common properties that are required for all log messages.
    /// </summary>
    [XmlRoot("logEntry")]
    [Serializable]
    public class PerformanceEntry : ICloneable
    {
        private static readonly TextFormatter ToStringFormatter = new TextFormatter();

        private string _message = string.Empty;
        private string _title = string.Empty;
        private ICollection<string> _categories = new List<string>(0);
        private int _priority = -1;
        private int _eventId;
        private Guid _activityId;
        private Guid? _relatedActivityId;

        private TraceEventType _severity = TraceEventType.Information;

        private string _machineName = string.Empty;
        private DateTime _timeStamp = DateTime.MaxValue;

        private StringBuilder _errorMessages;
        private IDictionary<string, object> _extendedProperties;

        private string _appDomainName;
        private string _processId;
        private string _processName;
        private string _threadName;
        private string _win32ThreadId;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceEntry"/> class. 
        /// Initialize a new instance of a <see cref="PerformanceEntry"/> class.
        /// </summary>
        public PerformanceEntry()
        {
            CollectIntrinsicProperties();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceEntry"/> class. 
        /// Create a new instance of <see cref="PerformanceEntry"/> with a full set of constructor parameters
        /// </summary>
        /// <param name="message">
        /// Message body to log.  Value from ToString() method from message object.
        /// </param>
        /// <param name="category">
        /// Category name used to route the log entry to a one or more trace listeners.
        /// </param>
        /// <param name="priority">
        /// Only messages must be above the minimum priority are processed.
        /// </param>
        /// <param name="eventId">
        /// Event number or identifier.
        /// </param>
        /// <param name="severity">
        /// Log entry severity as a <see cref="Severity"/> enumeration. (Unspecified, Information, Warning or Error).
        /// </param>
        /// <param name="title">
        /// Additional description of the log entry message.
        /// </param>
        /// <param name="properties">
        /// Dictionary of key/value pairs to record.
        /// </param>
        public PerformanceEntry(object message, string category, int priority, int eventId,
        TraceEventType severity, string title, IDictionary<string, object> properties)
            : this(message, BuildCategoriesCollection(category), priority, eventId, severity, title, properties)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceEntry"/> class. 
        /// Create a new instance of <see cref="PerformanceEntry"/> with a full set of constructor parameters
        /// </summary>
        /// <param name="message">
        /// Message body to log.  Value from ToString() method from message object.
        /// </param>
        /// <param name="categories">
        /// Collection of category names used to route the log entry to a one or more sinks.
        /// </param>
        /// <param name="priority">
        /// Only messages must be above the minimum priority are processed.
        /// </param>
        /// <param name="eventId">
        /// Event number or identifier.
        /// </param>
        /// <param name="severity">
        /// Log entry severity as a <see cref="Severity"/> enumeration. (Unspecified, Information, Warning or Error).
        /// </param>
        /// <param name="title">
        /// Additional description of the log entry message.
        /// </param>
        /// <param name="properties">
        /// Dictionary of key/value pairs to record.
        /// </param>
        public PerformanceEntry(object message, ICollection<string> categories, int priority, int eventId,
        TraceEventType severity, string title, IDictionary<string, object> properties)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            if (categories == null)
                throw new ArgumentNullException("categories");

            Message = message.ToString();
            Priority = priority;
            Categories = categories;
            EventId = eventId;
            Severity = severity;
            Title = title;
            ExtendedProperties = properties;

            CollectIntrinsicProperties();
        }

        /// <summary>
        /// Gets or sets Message body to log. Value from ToString() method from message object.
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        /// <summary>
        /// Gets or sets Category name used to route the log entry to a one or more trace listeners.
        /// </summary>
        public ICollection<string> Categories
        {
            get { return _categories; }
            set { _categories = value; }
        }

        /// <summary>
        /// Gets or sets Importance of the log message.  Only messages whose priority is between the minimum and maximum priorities (inclusive)
        /// will be processed.
        /// </summary>
        public int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        /// <summary>
        /// Gets or sets Event number or identifier.
        /// </summary>
        public int EventId
        {
            get { return _eventId; }
            set { _eventId = value; }
        }

        /// <summary>
        /// Gets or sets Log entry severity as a <see cref="Severity"/> enumeration. (Unspecified, Information, Warning or Error).
        /// </summary>
        public TraceEventType Severity
        {
            get { return _severity; }
            set { _severity = value; }
        }

        /// <summary>
        /// <para>Gets the string representation of the <see cref="Severity"/> enumeration.</para>
        /// </summary>
        /// <value>
        /// <para>The string value of the <see cref="Severity"/> enumeration.</para>
        /// </value>
        public string LoggedSeverity
        {
            get { return _severity.ToString(); }
        }

        /// <summary>
        /// Gets or sets Additional description of the log entry message.
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        /// <summary>
        /// Gets or sets Date and time of the log entry message.
        /// </summary>
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        /// <summary>
        /// Gets or sets Name of the computer.
        /// </summary>
        public string MachineName
        {
            get { return _machineName; }
            set { _machineName = value; }
        }

        /// <summary>
        /// Gets or sets The <see cref="AppDomain"/> in which the program is running
        /// </summary>
        public string AppDomainName
        {
            get { return _appDomainName; }
            set { _appDomainName = value; }
        }

        /// <summary>
        /// Gets or sets The Win32 process ID for the current running process.
        /// </summary>
        public string ProcessId
        {
            get { return _processId; }
            set { _processId = value; }
        }

        /// <summary>
        /// Gets or sets The name of the current running process.
        /// </summary>
        public string ProcessName
        {
            get { return _processName; }
            set { _processName = value; }
        }

        /// <summary>
        /// Gets or sets The name of the .NET thread.
        /// </summary>
        /// <seealso cref="Win32ThreadId"/>
        public string ManagedThreadName
        {
            get { return _threadName; }
            set { _threadName = value; }
        }

        /// <summary>
        /// Gets or sets The Win32 Thread ID for the current thread.
        /// </summary>
        public string Win32ThreadId
        {
            get { return _win32ThreadId; }
            set { _win32ThreadId = value; }
        }

        /// <summary>
        /// Gets or sets Dictionary of key/value pairs to record.
        /// </summary>
        public IDictionary<string, object> ExtendedProperties
        {
            get { return _extendedProperties ?? (_extendedProperties = new Dictionary<string, object>()); }
            set { _extendedProperties = value; }
        }

        /// <summary>
        /// Gets Read-only property that returns the timeStamp formatted using the current culture.
        /// </summary>
        public string TimeStampString
        {
            get { return TimeStamp.ToString(CultureInfo.CurrentCulture); }
        }

        /// <summary>
        /// Gets or sets Tracing activity id
        /// </summary>
        public Guid ActivityId
        {
            get { return _activityId; }
            set { _activityId = value; }
        }

        /// <summary>
        /// Gets or sets Related activity id
        /// </summary>
        public Guid? RelatedActivityId
        {
            get { return _relatedActivityId; }
            set { _relatedActivityId = value; }
        }

        /// <summary>
        /// Gets the error message with the <see cref="PerformanceEntry"></see>
        /// </summary>
        public string ErrorMessages
        {
            get
            {
                if (_errorMessages == null)
                    return null;
                return _errorMessages.ToString();
            }
        }

        /// <summary>
        /// Gets Tracing activity id as a string to support WMI Queries
        /// </summary>
        public string ActivityIdString
        {
            get { return ActivityId.ToString(); }
        }

        /// <summary>
        /// Gets Category names used to route the log entry to a one or more trace listeners.
        /// This readonly property is available to support WMI queries
        /// </summary>
        public string[] CategoriesStrings
        {
            get
            {
                var categoriesStrings = new string[Categories.Count];
                Categories.CopyTo(categoriesStrings, 0);
                return categoriesStrings;
            }
        }

        /// <summary>
        /// Gets the current process name.
        /// </summary>
        /// <returns>The process name.</returns>
        public static string GetProcessName()
        {
            return Process.GetCurrentProcess().ProcessName;
        }

        /// <summary>
        /// Creates a new <see cref="PerformanceEntry"/> that is a copy of the current instance.
        /// </summary>
        /// <remarks>
        /// If the dictionary contained in <see cref="ExtendedProperties"/> implements <see cref="ICloneable"/>, the resulting
        /// <see cref="PerformanceEntry"/> will have its ExtendedProperties set by calling <c>Clone()</c>. Otherwise the resulting
        /// <see cref="PerformanceEntry"/> will have its ExtendedProperties set to <see langword="null"/>.
        /// </remarks>
        /// <implements>ICloneable.Clone</implements>
        /// <returns>A new <c>PerformanceEntry</c> that is a copy of the current instance.</returns>
        public object Clone()
        {
            var result = new PerformanceEntry
            {
                Message = Message,
                EventId = EventId,
                Title = Title,
                Severity = Severity,
                Priority = Priority,
                TimeStamp = TimeStamp,
                MachineName = MachineName,
                AppDomainName = AppDomainName,
                ProcessId = ProcessId,
                ProcessName = ProcessName,
                ManagedThreadName = ManagedThreadName,
                ActivityId = ActivityId,
                Categories = new List<string>(Categories)
            };

            // clone categories

            // clone extended properties
            if (_extendedProperties != null)
                result.ExtendedProperties = new Dictionary<string, object>(_extendedProperties);

            // clone error messages
            if (_errorMessages != null)
            {
                result._errorMessages = new StringBuilder(_errorMessages.ToString());
            }

            return result;
        }

        /// <summary>
        /// Add an error or warning message to the start of the messages string builder.
        /// </summary>
        /// <param name="message">Message to be added to this instance</param>
        public virtual void AddErrorMessage(string message)
        {
            if (_errorMessages == null)
            {
                _errorMessages = new StringBuilder();
            }

            _errorMessages.Insert(0, Environment.NewLine);
            _errorMessages.Insert(0, Environment.NewLine);
            _errorMessages.Insert(0, message);
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="PerformanceEntry"/>, 
        /// using a default formatting template.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="PerformanceEntry"/>.</returns>
        public override string ToString()
        {
            return ToStringFormatter.Format(this);
        }

        protected static ICollection<string> BuildCategoriesCollection(string category)
        {
            if (string.IsNullOrEmpty(category))
                throw new ArgumentNullException("category");

            return new[] { category };
        }

        private static Guid GetActivityId()
        {
            return Trace.CorrelationManager.ActivityId;
        }

        private static string GetCurrentProcessId()
        {
            return Process.GetCurrentProcess().Id.ToString(NumberFormatInfo.InvariantInfo);
        }

        private static string GetCurrentThreadId()
        {
            return Thread.CurrentThread.GetHashCode().ToString(NumberFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Set the intrinsic properties such as MachineName and UserIdentity.
        /// </summary>
        private void CollectIntrinsicProperties()
        {
            TimeStamp = DateTime.UtcNow;

            if (PerformanceTracer.IsTracingAvailable())
            {
                try
                {
                    ActivityId = GetActivityId();
                }
                catch (Exception)
                {
                    ActivityId = Guid.Empty;
                }
            }
            else
            {
                ActivityId = Guid.Empty;
            }

            // do not try to avoid the security exception, as it would only duplicate the stack walk
            try
            {
                MachineName = Environment.MachineName;
            }
            catch (Exception e)
            {
                MachineName = String.Format("Error message: {0}", e.Message);
            }

            try
            {
                _appDomainName = AppDomain.CurrentDomain.FriendlyName;
            }
            catch (Exception e)
            {
                _appDomainName = String.Format("Error message: {0}", e.Message);
            }

            // check whether the unmanaged code permission is available to avoid three potential stack walks
            bool unmanagedCodePermissionAvailable = false;
            var unmanagedCodePermission = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);

            // avoid a stack walk by checking for the permission on the current assembly. this is safe because there are no
            // stack walk modifiers before the call.
            if (SecurityManager.IsGranted(unmanagedCodePermission))
            {
                try
                {
                    unmanagedCodePermission.Demand();
                    unmanagedCodePermissionAvailable = true;
                }
                catch (SecurityException)
                {
                }
            }

            if (unmanagedCodePermissionAvailable)
            {
                try
                {
                    _processId = GetCurrentProcessId();
                }
                catch (Exception e)
                {
                    _processId = String.Format("Error message: {0}", e.Message);
                }

                try
                {
                    _processName = GetProcessName();
                }
                catch (Exception e)
                {
                    _processName = String.Format("Error message: {0}", e.Message);
                }

                try
                {
                    _win32ThreadId = GetCurrentThreadId();
                }
                catch (Exception e)
                {
                    _win32ThreadId = String.Format("Error message: {0}", e.Message);
                }
            }
            else
            {
                _processId = String.Format("Error message: {0}", "Permission for UnmanagedCode is not available");
                _processName = String.Format("Error message: {0}", "Permission for UnmanagedCode is not available");
                _win32ThreadId = String.Format("Error message: {0}", "Permission for UnmanagedCode is not available");
            }

            try
            {
                _threadName = Thread.CurrentThread.Name;
            }
            catch (Exception e)
            {
                _threadName = String.Format("Error message: {0}", e.Message);
            }
        }
    }
}