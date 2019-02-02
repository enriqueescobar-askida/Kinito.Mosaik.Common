
namespace Translator2009.Installer
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class WinInfo
    {
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool lpSystemInfo);


        public readonly WinVersion WindowsVersion;


        public static WinType WinType
        {
            get
            {
                return (WinInfo.IsSystem64Bit()) ? WinType.x64 : WinType.x86;
            }
        }


        public WinInfo(WinVersion windowsVersion)
        {
            this.WindowsVersion = windowsVersion;
        }

        public static WinInfo GetWinInfo()
        {

            OperatingSystem os = System.Environment.OSVersion;

            switch (os.Platform)
            {
                case PlatformID.Win32NT:
                    switch (os.Version.Major)
                    {
                        case 4:
                            // Windows NT
                            return new WinInfo(
                                WinVersion.WinNT4);
                        case 5:
                            if (os.Version.Minor == 0)
                            {
                                // Windows 2000
                                return new WinInfo(
                                    WinVersion.Win2000);
                            }
                            else
                            {
                                // Windows XP
                                return new WinInfo(
                                    WinVersion.WinXP);
                            }
                        default: // 6
                            // Vista
                            return new WinInfo(
                                WinVersion.Vista);
                    }

                case PlatformID.Win32Windows:
                    switch (os.Version.Minor)
                    {
                        case 0:
                            // Windows 95
                            return new WinInfo(
                                WinVersion.Win95);
                        case 10:
                            // Windows 98
                            return new WinInfo(
                                WinVersion.Win98);
                        default: // 90
                            // Windows ME
                            return new WinInfo(
                                WinVersion.WinME);
                    }
            }

            throw new NotSupportedException("Operating system not supported.");
        }


        public static bool IsSystem64Bit()
        {
            return (IntPtr.Size == 8 || (IntPtr.Size == 4 && Is32BitProcessOn64BitProcessor()));
        }

        private static bool Is32BitProcessOn64BitProcessor()
        {
            bool retVal = false;
            try
            {
                IsWow64Process(Process.GetCurrentProcess().Handle, out retVal);
            }
            catch
            {
            }
            return retVal;
        }
    }
}
