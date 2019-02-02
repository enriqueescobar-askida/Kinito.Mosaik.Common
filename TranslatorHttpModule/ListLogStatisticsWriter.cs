// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListLogStatisticsWriter.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the ListLogStatisticsWriter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Text;
using Alphamosaik.Common.Library.Statistics;
using Alphamosaik.Common.SharePoint.Library.ConfigStore;

namespace TranslatorHttpHandler
{
    public class ListLogStatisticsWriter : IStatisticsWriter
    {
        private static readonly object LockObject = new object();
        private readonly string _itemName;
        private readonly string _absoluteUri;
        private readonly StringBuilder _buffer = new StringBuilder();

        public ListLogStatisticsWriter(string itemName, string absoluteUri)
        {
            _itemName = itemName;
            _absoluteUri = absoluteUri;
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
                    EventLog.WriteEntry("ListLogStatisticsWriter", ex.Message + Environment.NewLine + ex, EventLogEntryType.Warning);
                }
            }
        }

        public void Flush()
        {
            lock (LockObject)
            {
                try
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(_buffer.ToString());
                    TroubleshootingStore.Instance.AddValue(_itemName, "Statistics", _itemName, string.Empty, _itemName + ".log", bytes, false, _absoluteUri);
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry("ListLogStatisticsWriter", ex.Message + Environment.NewLine + ex, EventLogEntryType.Warning);
                }
            }
        }
    }
}