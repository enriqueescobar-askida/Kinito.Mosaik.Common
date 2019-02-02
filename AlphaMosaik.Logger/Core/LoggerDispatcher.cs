using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlphaMosaik.Logger.Storage;
using System.Diagnostics;

namespace AlphaMosaik.Logger
{
    /// <summary>
    /// Offre des fonctionnalités qui permettent de journaliser les évènements de manière asynchrone
    /// </summary>
    public class LoggerDispatcher
    {
        public Logger ParentLogger { get; private set; }

        private IAsyncResult WorkStatus;
        private Queue<LogEntry> WaitingEntries;
        private object WorkMutex;
        private bool started;

        private PerformanceCounter queueSizeCounter;

        private delegate void AddEntryDelegate(LogEntry entry);

        public LoggerDispatcher(Logger parent)
        {
            ParentLogger = parent;
            WaitingEntries = new Queue<LogEntry>();
            WorkStatus = null;
            WorkMutex = new object();

            if (PerformanceCounterCategory.Exists("AlphaMosaik Logger") && PerformanceCounterCategory.CounterExists("QueueSize", "AlphaMosaik Logger")) // TODO : Propriétés
            {
                queueSizeCounter = new PerformanceCounter("AlphaMosaik Logger", "QueueSize"); // TODO : à remplacer par des propriétés
                queueSizeCounter.ReadOnly = false;
                queueSizeCounter.RawValue = 0;
            }
            else
            {
                ParentLogger.LoggerEventLog.WriteEntry("Performance counters cannot be found. Try to repair the installation.", EventLogEntryType.Error);
            }
        }

        public void Start()
        {
            started = true;

            lock (WorkMutex)
            {
                if (WaitingEntries.Count > 0)
                {
                    if (WorkStatus == null || WorkStatus.IsCompleted)
                    {
                        LogEntry nextEntry = WaitingEntries.Dequeue();
                        queueSizeCounter.Decrement();

                        AddEntryDelegate d = AddEntry;
                        WorkStatus = d.BeginInvoke(nextEntry, AddEntryCallBack, null);
                    }
                }
            }
        }

        public void Pause()
        {
            started = false;
            WorkStatus.AsyncWaitHandle.WaitOne(); // On attend qu'un éventuel traitement se termine
        }

        /// <summary>
        /// Soumet un évènement qui sera enregistré de manière asynchrone.
        /// </summary>
        /// <param name="entry">Evènement à journaliser</param>
        public void SubmitEntry(LogEntry entry)
        {
            lock (WorkMutex)
            {
                WaitingEntries.Enqueue(entry);
                queueSizeCounter.Increment();
            }

            if (started)
            {
                Start();
            }
        }

        /// <summary>
        /// Traitement de l'évènement
        /// </summary>
        /// <param name="entry"></param>
        private void AddEntry(LogEntry entry)
        {
            ParentLogger.StorageManager.AddEntry(entry);
        }

        /// <summary>
        /// Une fois que le traitement est terminé :
        ///     - plus de LogEntry à traiter, on ne fait rien
        ///     - il reste des LogEntry, on dépile et on exécute le traitement asynchrone.
        ///       cela peut continuer tant qu'il reste des éléments dans la queue.
        /// </summary>
        /// <param name="ar"></param>
        private void AddEntryCallBack(IAsyncResult ar)
        {
            ar.AsyncWaitHandle.WaitOne();   // On attend que le traitement soit terminé

            if(started) // Si on est toujours dans un état démarré on traite l'entrée suivante
            {
                Start();
            }
        }
    }
}
