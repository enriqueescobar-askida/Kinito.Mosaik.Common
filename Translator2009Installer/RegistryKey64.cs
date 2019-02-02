
namespace Translator2009.Installer
{
    using System;
    using Microsoft.Win32;
    using System.Reflection;
    using System.Collections.Generic;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class RegistryKey64
    {

        public const int KEY_QUERY_VALUE = 0x1;
        public const int KEY_WOW64_64KEY = 0x0100;
        public const int KEY_WOW64_32KEY = 0x0200;

        private RegistryKey _subKey = null;
        private readonly Type keyType = typeof(RegistryKey);

        public RegistryKey SubKey
        {
            get { return _subKey; }
            set { _subKey = value; }
        }

        public RegistryKey64(RegistryKey subKey)
        {
            this.SubKey = subKey;
        }

        public static RegistryKey OpenSubKey(RegistryKey subKey, string name)
        {
            return OpenSubKey(subKey, name, WinInfo.WinType);
        }

        public static RegistryKey OpenSubKey(RegistryKey subKey, string name, WinType winType)
        {
            RegistryKey64 reg = new RegistryKey64(subKey);

            int wow6432 = (winType == WinType.x64) ? KEY_WOW64_64KEY : KEY_WOW64_32KEY;

            return reg.OpenSubKey(name, KEY_QUERY_VALUE + wow6432);
        }


        public RegistryKey OpenSubKey(string name, int samDesired)
        {
            object hkResult = null;
            object safeRegistryHandleKey = GetFieldValue(SubKey, "hkey");
            object[] parameters = new object[] {
                safeRegistryHandleKey,
                name,
                0, 
                samDesired,
                hkResult
            };

            int num = InvokeRegOpenKeyEx(parameters);

            hkResult = parameters[4];

            if (num == 0)
            {
                RegistryKey key = CreateKey(hkResult, false);

                SetFieldValue(key, "checkMode", InvokeGetSubKeyPermissonCheck(key, false));

                string keyName = (string)GetFieldValue(key, "keyName");
                SetFieldValue(key, "keyName", keyName + @"\" + name);

                return key;
            }
            if ((num == 5) || (num == 0x542))
            {
                throw new ApplicationException("Security issue!");
            }
            return null;
        }

        //
        //
        // MSCORLIB 2.0 - Used in VS2005 and 2008
        // private RegistryKey(SafeRegistryHandle hkey, bool writable);
        // private RegistryKey(SafeRegistryHandle hkey, bool writable, bool systemkey, bool remoteKey, bool isPerfData);


        // MSCORLIB 4.0 - Used in VS2010 Beta 2
        // private RegistryKey(SafeRegistryHandle hkey, bool writable, RegistryView view);
        // private RegistryKey(SafeRegistryHandle hkey, bool writable, bool systemkey, bool remoteKey, bool isPerfData, RegistryView view);
        //
        // The RegistryView parameter is added when mscorlib 4.0 are used!
        // public enum RegistryView
        // {
        //    Default = 0,
        //    Registry32 = 0x200,
        //    Registry64 = 0x100
        // }
        private RegistryKey CreateKey(object hkResult, bool writable)
        {
            bool remoteKey = (bool)GetFieldValue(SubKey, "remoteKey");
            List<object> parameters = new List<object>(new object[] {
                hkResult,
                writable,
                false,
                remoteKey,
                false });

            if (keyType.Assembly.GetName().Version.Major >= 4)
            {
                Type viewType = Type.GetType("Microsoft.Win32.RegistryView");
                Array obj = Enum.GetValues(viewType);
                parameters.Add(obj.GetValue(0));
            }

            RegistryKey key = (RegistryKey)Activator.CreateInstance(
                keyType,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, parameters.ToArray(),
                null);

            return key;
        }

        private object GetFieldValue(RegistryKey key, string name)
        {
            FieldInfo f = keyType.GetField(name, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic);
            return f.GetValue(SubKey);
        }

        private void SetFieldValue(RegistryKey key, string name, object value)
        {
            FieldInfo f = keyType.GetField(name, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic);
            f.SetValue(SubKey, value);
        }

        private object InvokeGetSubKeyPermissonCheck(RegistryKey key, bool writable)
        {
            MethodInfo m = keyType.GetMethod("GetSubKeyPermissonCheck", BindingFlags.NonPublic | BindingFlags.Instance);
            return m.Invoke(SubKey, new object[] { writable });
        }

        private int InvokeRegOpenKeyEx(object[] parameters)
        {
            Type win32Type = Type.GetType("Microsoft.Win32.Win32Native");

            //__in        HKEY hKey,
            //__in_opt    LPCTSTR lpSubKey,
            //__reserved  DWORD ulOptions,
            //__in        REGSAM samDesired,
            //__out       PHKEY phkResult

            int result = 0;
            MethodInfo[] methods = win32Type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
            foreach (MethodInfo method in methods)
            {
                if (method.Name.EndsWith("RegOpenKeyEx", StringComparison.Ordinal))
                {
                    ParameterInfo[] methodParameters = method.GetParameters();
                    if (methodParameters.Length == parameters.Length)
                    {
                        result = (int)method.Invoke(null, parameters);
                        break;
                    }
                }
            }

            return result;
        }

    }
}
