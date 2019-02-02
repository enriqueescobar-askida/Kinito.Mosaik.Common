// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextFormatter.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Represents a template based formatter for <see cref="PerformanceEntry" /> messages.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Alphamosaik.Common.Library.Performance
{
    /// <summary>
    /// Represents a template based formatter for <see cref="PerformanceEntry"/> messages.
    /// </summary>
    public class TextFormatter : LogFormatter
    {
        private const string TimeStampToken = "{timestamp}";
        private const string MessageToken = "{message}";
        private const string CategoryToken = "{category}";
        private const string PriorityToken = "{priority}";
        private const string EventIdToken = "{eventid}";
        private const string SeverityToken = "{severity}";
        private const string TitleToken = "{title}";
        private const string ErrorMessagesToke = "{errorMessages}";

        private const string MachineToken = "{machine}";
        private const string AppDomainNameToken = "{appDomain}";
        private const string ProcessIdToken = "{processId}";
        private const string ProcessNameToken = "{processName}";
        private const string ThreadNameToken = "{threadName}";
        private const string Win32ThreadIdToken = "{win32ThreadId}";
        private const string ActivityidToken = "{activity}";

        private const string NewLineToken = "{newline}";
        private const string TabToken = "{tab}";

        private const string DefaultTextFormat = "Timestamp: {timestamp}{newline}Message: {message}{newline}Category: {category}{newline}Priority: {priority}{newline}EventId: {eventid}{newline}Severity: {severity}{newline}Title:{title}{newline}Machine: {machine}{newline}App Domain: {appDomain}{newline}ProcessId: {processId}{newline}Process Name: {processName}{newline}Thread Name: {threadName}{newline}Win32 ThreadId:{win32ThreadId}{newline}Extended Properties: {dictionary({key} - {value}{newline})}.";

        /// <summary>
        /// Message template containing tokens.
        /// </summary>
        private string _template;

        /// <summary>
        /// Array of token formatters.
        /// </summary>
        private ArrayList _tokenFunctions;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFormatter"/> class. 
        /// Initializes a new instance of a <see cref="TextFormatter"></see>
        /// </summary>
        /// <param name="template">
        /// Template to be used when formatting.
        /// </param>
        public TextFormatter(string template)
        {
            _template = !string.IsNullOrEmpty(template) ? template : DefaultTextFormat;
            RegisterTokenFunctions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFormatter"/> class. 
        /// Initializes a new instance of a <see cref="TextFormatter"></see> with a default template.
        /// </summary>
        public TextFormatter()
            : this(DefaultTextFormat)
        {
        }

        /// <summary>
        /// Gets or sets the formatting template.
        /// </summary>
        public string Template
        {
            get { return _template; }
            set { _template = value; }
        }

        /// <summary>
        /// Provides a textual representation of a categories list.
        /// </summary>
        /// <param name="categories">The collection of categories.</param>
        /// <returns>A comma delimited textural representation of the categories.</returns>
        public static string FormatCategoriesCollection(ICollection<string> categories)
        {
            var categoriesListBuilder = new StringBuilder();
            int i = 0;
            foreach (string category in categories)
            {
                categoriesListBuilder.Append(category);
                if (++i < categories.Count)
                {
                    categoriesListBuilder.Append(", ");
                }
            }

            return categoriesListBuilder.ToString();
        }

        /// <overloads>
        /// Formats the <see cref="PerformanceEntry"/> object by replacing tokens with values
        /// </overloads>
        /// <summary>
        /// Formats the <see cref="PerformanceEntry"/> object by replacing tokens with values.
        /// </summary>
        /// <param name="performance">Log entry to format.</param>
        /// <returns>Formatted string with tokens replaced with property values.</returns>
        public override string Format(PerformanceEntry performance)
        {
            return Format(CreateTemplateBuilder(), performance);
        }

        /// <summary>
        /// Formats the <see cref="PerformanceEntry"/> object by replacing tokens with values writing the format result
        /// to a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="templateBuilder">The <see cref="StringBuilder"/> that holds the formatting result.</param>
        /// <param name="performance">Log entry to format.</param>
        /// <returns>Formatted string with tokens replaced with property values.</returns>
        protected virtual string Format(StringBuilder templateBuilder, PerformanceEntry performance)
        {
            templateBuilder.Replace(TimeStampToken, performance.TimeStampString);
            templateBuilder.Replace(TitleToken, performance.Title);
            templateBuilder.Replace(MessageToken, performance.Message);
            templateBuilder.Replace(EventIdToken, performance.EventId.ToString());
            templateBuilder.Replace(PriorityToken, performance.Priority.ToString());
            templateBuilder.Replace(SeverityToken, performance.Severity.ToString());
            templateBuilder.Replace(ErrorMessagesToke, performance.ErrorMessages);

            templateBuilder.Replace(MachineToken, performance.MachineName);
            templateBuilder.Replace(AppDomainNameToken, performance.AppDomainName);
            templateBuilder.Replace(ProcessIdToken, performance.ProcessId);
            templateBuilder.Replace(ProcessNameToken, performance.ProcessName);
            templateBuilder.Replace(ThreadNameToken, performance.ManagedThreadName);
            templateBuilder.Replace(Win32ThreadIdToken, performance.Win32ThreadId);
            templateBuilder.Replace(ActivityidToken, performance.ActivityId.ToString("D"));

            templateBuilder.Replace(CategoryToken, FormatCategoriesCollection(performance.Categories));

            FormatTokenFunctions(templateBuilder, performance);

            templateBuilder.Replace(NewLineToken, Environment.NewLine);
            templateBuilder.Replace(TabToken, "\t");

            return templateBuilder.ToString();
        }

        /// <summary>
        /// Creates a new builder to hold the formatting results based on the receiver's template.
        /// </summary>
        /// <returns>The new <see cref="StringBuilder"/>.</returns>
        protected StringBuilder CreateTemplateBuilder()
        {
            var templateBuilder =
            new StringBuilder((_template == null) || (_template.Length > 0) ? _template : DefaultTextFormat);
            return templateBuilder;
        }

        private void FormatTokenFunctions(StringBuilder templateBuilder, PerformanceEntry performance)
        {
            foreach (TokenFunction token in _tokenFunctions)
            {
                token.Format(templateBuilder, performance);
            }
        }

        private void RegisterTokenFunctions()
        {
            _tokenFunctions = new ArrayList
                                            {
                                            new DictionaryToken(),
                                            new KeyValueToken(),
                                            new TimeStampToken(),
                                            new ReflectedPropertyToken()
                                            };
        }
    }
}