﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AlphaMosaik.Logger.Providers.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AlphaTextLogProvider.IsEnabled")]
        public string KeyAlphaTextLogProviderIsEnabled {
            get {
                return ((string)(this["KeyAlphaTextLogProviderIsEnabled"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AlphaTextLogProvider.FilePath")]
        public string KeyAlphaTextLogProviderFilePath {
            get {
                return ((string)(this["KeyAlphaTextLogProviderFilePath"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AlphaSqlLogProviderConnectionString")]
        public string AlphaSqlLogProviderConnectionStringName {
            get {
                return ((string)(this["AlphaSqlLogProviderConnectionStringName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AlphaSqlLogProvider.IsEnabled")]
        public string KeyAlphaSqlLogProviderIsEnabled {
            get {
                return ((string)(this["KeyAlphaSqlLogProviderIsEnabled"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AlphaSpTraceLogProvider.IsEnabled")]
        public string KeyAlphaSpTraceLogProviderIsEnabled {
            get {
                return ((string)(this["KeyAlphaSpTraceLogProviderIsEnabled"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=.\\OFFICESERVERS;Initial Catalog=Logger;Integrated Security=True")]
        public string LoggerConnectionString {
            get {
                return ((string)(this["LoggerConnectionString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AlphaTextLogProvider.MaxFileSize")]
        public string KeyAlphaTextLogProviderMaxFileSize {
            get {
                return ((string)(this["KeyAlphaTextLogProviderMaxFileSize"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AlphaEventLogProvider.IsEnabled")]
        public string KeyAlphaEventLogProviderIsEnabled {
            get {
                return ((string)(this["KeyAlphaEventLogProviderIsEnabled"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AlphaMosaik")]
        public string AlphaEventLogProviderSourceName {
            get {
                return ((string)(this["AlphaEventLogProviderSourceName"]));
            }
        }
    }
}
