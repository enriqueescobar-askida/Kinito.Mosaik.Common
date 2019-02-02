using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Diagnostics;


namespace AlphaMosaik.Logger.WindowsService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        private EventLogInstaller eventLogInstaller;

        public ProjectInstaller()
        {
            InitializeComponent();

            // INSTALLATION DU JOURNAL D'EVENEMENTS POUR LE PROVIDER AlphaLogEventProvider
            //if (!EventLog.SourceExists("AlphaMosaikLoggerProvider"))
            //{
            //    eventLogInstaller = new EventLogInstaller();
            //    eventLogInstaller.Log = "AlphaMosaik";
            //    eventLogInstaller.Source = "AlphaMosaikLoggerProvider";
            //    //Installers.Add(eventLogInstaller);
            //}

            //// INSTALLATION DES COMPTEURS DE PERFORMANCE
            //if (!(PerformanceCounterCategory.Exists("AlphaMosaik Logger") && PerformanceCounterCategory.CounterExists("QueueSize", "AlphaMosaik Logger")))
            //{
            //    PerformanceCounterCategory.Create("AlphaMosaik Logger",         // Nom de la catégorie
            //                    "Counters of AlphaMosaik Logger",               // Description de la catégorie
            //                    PerformanceCounterCategoryType.SingleInstance,  // Type de compteur
            //                    "QueueSize",                                    // Nom du compteur
            //                    "Number of items waiting to be treated.");      // Description du compteur

            //    PerformanceCounterInstaller counterInstaller = new PerformanceCounterInstaller();
            //    counterInstaller.CategoryName = "AlphaMosaik Logger";
            //    counterInstaller.CategoryHelp = "Counters of AlphaMosaik Logger";

            //    CounterCreationData queueSizeCounter = new CounterCreationData();
            //    queueSizeCounter.CounterName = "QueueSize";
            //    queueSizeCounter.CounterHelp = "Numbers of events waiting to be treated";
            //    queueSizeCounter.CounterType = PerformanceCounterType.NumberOfItems32;

            //    counterInstaller.Counters.Add(queueSizeCounter);

            //    Installers.Add(counterInstaller);
            //}
        }
    }
}
