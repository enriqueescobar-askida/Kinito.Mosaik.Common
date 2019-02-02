
namespace Translator2009.Installer
{
    using System;

    using Microsoft.Win32;

    public class SharePointRegistry
    {
        #region constants
        public const string SHAREPOINT_12 = "12";
        public const string SHAREPOINT_14 = "14";

        public const string SHAREPOINT_KEY = @"SOFTWARE\Microsoft\Shared Tools\Web Server Extensions\";

        #endregion

        #region fields

        RegistryKey _sharepointKey = null;
        RegistryKey _visualStudioKey = null;
        private string _sharePointRootLocation = null;



        #endregion

        #region public properties

        public string SharePointRootLocation
        {
            get
            {
                if (this._sharePointRootLocation == null)
                {
                    this._sharePointRootLocation = GetValue("Location");

                    if (string.IsNullOrEmpty(this._sharePointRootLocation))
                    {
                        throw new ApplicationException("SharePoint hive path not found on the system!");
                    }

                    if (!this._sharePointRootLocation.EndsWith("\\"))
                    {
                        this._sharePointRootLocation += "\\";
                    }

                }
                return _sharePointRootLocation;
            }
        }

        private string _version = null;
        public string Version
        {
            get
            {
                if (String.IsNullOrEmpty(_version))
                {
                    _version = GetValue("Version") + string.Empty;
                }
                return _version;
            }
        }

        #endregion

        #region constructor & descructor

        SharePointRegistry()
        {

        }

        ~SharePointRegistry()
        {
            if (_sharepointKey != null)
            {
                _sharepointKey.Close();
            }
            if (_visualStudioKey != null)
            {
                _visualStudioKey.Close();
            }
        }

        #endregion


        #region public static methods

        public static bool IsSharePoint12
        {
            get
            {
                return Instance.Version.StartsWith(SHAREPOINT_12);
            }
        }


        public static bool IsSharePoint14
        {
            get
            {
                return Instance.Version.StartsWith(SHAREPOINT_14);
            }
        }


        //[RegistryPermission(SecurityAction.Assert, Read = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Shared Tools\Web Server Extensions")]
        internal static T GetLocalMachineRegistryValue<T>(string path, string keyName, T defaultValue, WinType winType)
        {
            T local = default(T);

            RegistryKey key = null;
            //try
            //{
            key = RegistryKey64.OpenSubKey(Registry.LocalMachine, path, winType);
            //try
            //{
            //}
            //catch
            //{
            //    key = Registry.LocalMachine.OpenSubKey(path, false);
            //}

            //key = Registry.LocalMachine.OpenSubKey(path);

            if (key == null)
            {
                return defaultValue;
            }
            object obj2 = key.GetValue(keyName);
            if (obj2 is T)
            {
                return (T)obj2;
            }
            local = defaultValue;
            //}
            //catch
            //{
            //    key.Close();
            //}


            return local;
        }

        public static string GetValue(string subkey)
        {
            string value = GetValue(subkey, SHAREPOINT_KEY + SHAREPOINT_12 + ".0");
            if (value == null)
            {
                value = GetValue(subkey, SHAREPOINT_KEY + SHAREPOINT_14 + ".0");
            }
            return value;
        }

        public static string GetValue(string subkey, string path)
        {
            string value = GetLocalMachineRegistryValue<string>(path, subkey, null, WinInfo.WinType);
            if (value == null)
            {
                //Flip the WinType and try again on the other registry (x86/x64) 
                WinType winType = (WinInfo.WinType == WinType.x86) ? WinType.x64 : WinType.x86;
                value = GetLocalMachineRegistryValue<string>(path, subkey, null, winType);
            }

            return value;
        }



        #endregion

        #region Singleton

        public static SharePointRegistry Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly SharePointRegistry instance = new SharePointRegistry();
        }

        #endregion
    }
}
