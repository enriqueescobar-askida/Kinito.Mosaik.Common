using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using AlphaMosaik.Logger.Configuration;
using AlphaMosaik.Logger.Storage;

namespace AlphaMosaik.Logger.Diagnostics
{
    public static class TraceHelper
    {
        public static void TraceAppSettings(AppSettingsSection appSettings)
        {
#if TRACE
            Trace.WriteLine("Le fichier de configuration contient les valeurs suivantes : [clé]=valeur");
            foreach (string key in appSettings.Settings.AllKeys)
            {
                Trace.WriteLine("[" + key + "]=\"" + appSettings.Settings[key].Value + "\"");
            }
#endif
        }

        public static void TraceProviderDefinitions(LoggerSection section)
        {
#if TRACE
            Trace.WriteLine("Les définitions de provider sont:");
            foreach (ProviderDefinitionElement providerDefinition in section.Definitions)
            {
                Trace.WriteLine(providerDefinition.Name);
            }
#endif
        }

        public static void TraceProviderSettings(LoggerSection section)
        {
#if TRACE
            Trace.WriteLine("Les paramètres des providers sont:");
            foreach (KeyValueConfigurationElement setting in section.ProviderSettings)
            {
                Trace.WriteLine("[" + setting.Key + "]=\"" + setting.Value + "\"");
            }
#endif
        }

        public static void TraceWriteEntry(LogEntry entry)
        {
#if TRACE
            Trace.WriteLine("Le message \"" + entry + "\" va être journalisé.");
#endif
        }

        public static void WriteLine(string message)
        {
#if TRACE
            Trace.WriteLine(message);
#endif
        }

        public static void TraceProvider(this IStorageProvider provider)
        {
#if TRACE
            Trace.WriteLine("Trace du provider : " + provider.Definition.Name);
            Trace.WriteLine("  Assembly: " + provider.Definition.Assembly);
            Trace.WriteLine("  Class: " + provider.Definition.Class);
            Trace.WriteLine("  Activé: " + provider.Enabled);
#endif
        }
    }
}
