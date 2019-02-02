using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlphaMosaik.Logger.Configuration;
using System.Reflection;
using System.Runtime.Remoting;
using AlphaMosaik.Logger.Diagnostics;
using AlphaMosaik.Logger.Core.Exceptions;
using System.Diagnostics;

namespace AlphaMosaik.Logger.Storage
{
    public class StorageManager : IDisposable
    {
        public List<IStorageProvider> StorageProviders { get; internal set; }
        public Logger ParentLogger { get; private set; }

        private List<IStorageProvider> enabledStorageProviders;
        private bool disposed;

        internal StorageManager(Logger parent)
        {
            ParentLogger = parent;
            Load();
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    foreach (IStorageProvider provider in StorageProviders)
                    {
                        if(provider is IDisposable)
                        {
                            IDisposable disposableProvider = (IDisposable)provider;
                            disposableProvider.Dispose();
                        }
                    }
                    StorageProviders.Clear(); // Est-ce utile?
                    enabledStorageProviders.Clear(); // Est-ce utile?
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Ajoute une entrée dans le système de journalisation.
        /// L'entrée est ajoutée dans tous les fournisseurs actifs.
        /// </summary>
        /// <param name="entry"></param>
        internal void AddEntry(LogEntry entry)
        {
            foreach (IStorageProvider enabledProvider in enabledStorageProviders)
            {
                enabledProvider.AddEntry(entry);
            }
        }

        private void Load()
        {
            ProviderDefinitionCollection definitionCollection = ParentLogger.ConfigurationManager.Logger.Definitions;
            StorageProviders = new List<IStorageProvider>(definitionCollection.Count);

            if (definitionCollection.Count > 0)
            {
                // On charge les storage providers
                foreach (ProviderDefinitionElement definition in definitionCollection)
                {
                    try
                    {
                        IStorageProvider provider = definition.CreateInstance();

                        // Chargement des informations de définition
                        provider.Name = definition.Name;
                        provider.Definition = definition;
                        
                        if (provider is BaseStorageProvider)
                        {
                            //Traitement spécifique aux providers qui héritent de BaseStorageProvider
                            BaseStorageProvider baseStorageProvider = (BaseStorageProvider)provider;
                            baseStorageProvider.ConfigurationManager = ParentLogger.ConfigurationManager;
                            baseStorageProvider.LoggerEventLog = ParentLogger.LoggerEventLog;

                            // Chargement de la configuration du provider
                            baseStorageProvider.LoadSettings();
                            baseStorageProvider.EnabledChanged += StorageProvider_EnabledChanged;

                            StorageProviders.Add(baseStorageProvider);
                        }
                        else
                        {
                            // Chargement de la configuration du provider
                            provider.LoadSettings();
                            provider.EnabledChanged += StorageProvider_EnabledChanged;

                            StorageProviders.Add(provider);
                        }
                        provider.TraceProvider();
                    }
                    catch (ProviderDefinitionException ex)
                    {
                        ParentLogger.LoggerEventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
                        continue;
                    }
                }
            }

            // On met à jour la liste des providers actifs
            enabledStorageProviders = (from p in StorageProviders
                                       where p.Enabled == true
                                       select p).ToList(); // TODO : prendre en compte le cas où aucun provider n'est disponible
        }

        private void StorageProvider_EnabledChanged(object sender, EventArgs eventArgs)
        {
            IStorageProvider storageProvider = (IStorageProvider)sender;
            if (storageProvider.Enabled)
            {
                enabledStorageProviders.Add(storageProvider);
            }
            else
            {
                enabledStorageProviders.Remove(storageProvider);
            }
        }
    }
}
