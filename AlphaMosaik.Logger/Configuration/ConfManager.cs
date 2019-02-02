using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using AlphaMosaik.Logger.Properties;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Configuration;
using System.Diagnostics;
using AlphaMosaik.Logger.Diagnostics;
using AlphaMosaik.Logger.Storage;

namespace AlphaMosaik.Logger.Configuration
{
    public class ConfManager
    {
        public LoggerSection Logger { get; internal set; }
        public ConnectionStringSettingsCollection ConnectionStrings { get; internal set; }

        protected Logger ParentLogger;

        private System.Configuration.Configuration config;
        private AppSettingsSection settingsSection;
        private FileSystemWatcher confFileWatcher;

        internal ConfManager(Logger parent)
        {
            ParentLogger = parent;

            LoadConfig();

            confFileWatcher = new FileSystemWatcher(Path.GetDirectoryName(config.FilePath), "*.config");
            confFileWatcher.IncludeSubdirectories = false;
            confFileWatcher.Changed += new FileSystemEventHandler(confFileWatcher_Changed);
            confFileWatcher.EnableRaisingEvents = true;
        }

        public void SaveSettings()
        {
            config.Save(ConfigurationSaveMode.Full);
        }

        protected void LoadConfig()
        {
            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            settingsSection = config.AppSettings;

            Logger = (LoggerSection)config.GetSection("logger");
            ConnectionStrings = config.ConnectionStrings.ConnectionStrings;
            config.Save(ConfigurationSaveMode.Minimal); // TODO : Vérifier si ca a une utilité

            TraceHelper.WriteLine("Le fichier de configuration utilisé est : " + config.FilePath);
            TraceHelper.TraceAppSettings(settingsSection);
            TraceHelper.TraceProviderDefinitions(Logger);
            TraceHelper.TraceProviderSettings(Logger);
        }

        void confFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            LoadConfig();
            ParentLogger.Reload();
        }
    }
}
