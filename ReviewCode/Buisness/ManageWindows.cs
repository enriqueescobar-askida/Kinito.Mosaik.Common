using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using EnvDTE80;
using EnvDTE;

namespace ReviewCode.Buisness
{
    public static class ManageWindows
    {
        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i);

        [DllImport("user32.dll")]
        static extern int GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(int hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }


        private static Window2 windowPendingChanges = null;
        private static DTE2 _applicationObject = null;
        private static System.Windows.Forms.DataGridView dataGridWorkItem = null;

        /// <summary>
        /// Returns a list of child windows
        /// </summary>
        /// <param name="parent">Parent of the windows to return</param>
        /// <returns>List of child windows</returns>
        public static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }
        /// <summary>
        /// Callback method to be used when enumerating windows.
        /// </summary>
        /// <param name="handle">Handle of the next window</param>
        /// <param name="pointer">Pointer to a GCHandle that holds a reference to the list to fill</param>
        /// <returns>True to continue the enumeration, false to bail</returns>
        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<IntPtr> list = gch.Target as List<IntPtr>;
            if (list == null)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }

        /// <summary>
        /// Delegate for the EnumChildWindows method
        /// </summary>
        /// <param name="hWnd">Window handle</param>
        /// <param name="parameter">Caller-defined variable; we use it for a pointer to our list</param>
        /// <returns>True to continue enumerating, false to bail.</returns>
        public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);


        public static IntPtr GetActiveWindow()
        {
            const int nChars = 256;
            int handle = 0;
            IntPtr windowIntPtr = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);

            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                System.Diagnostics.Trace.WriteLine("buff :" + Buff.ToString());
                System.Diagnostics.Trace.WriteLine("Handle :" + handle.ToString());
                windowIntPtr = new IntPtr(handle);
            }
            return windowIntPtr;
        }

        public static IntPtr GetWindowPtr(IntPtr handle)
        {
            IntPtr tmp = GetWindowLongPtr(handle, -12);
            return tmp;
        }

        /// <summary>
        /// Get VisualStudio GridView PendingChanges
        /// </summary>
        /// <param name="_appObject">DTE2 application object</param>
        /// <returns>DataGridView </returns>
        public static System.Windows.Forms.DataGridView GetVisualStudioGridViewPendingChanges(DTE2 _appObject)
        {
            _applicationObject = _appObject;

            var windows = _applicationObject.Windows.GetEnumerator();
            while (windows.MoveNext())
            {
                Window2 tmp = (Window2)windows.Current;
                System.Diagnostics.Trace.WriteLine("Test : " + tmp.Caption);

                if (tmp.Caption.Contains("Pending Changes"))
                {
                    windowPendingChanges = (Window2)tmp;
                }
            }

            if (windowPendingChanges != null)
            {
                windowPendingChanges.Activate();
                IntPtr windowIntPtr = Buisness.ManageWindows.GetActiveWindow();
                List<IntPtr> listChild = Buisness.ManageWindows.GetChildWindows(windowIntPtr);

                if (listChild.Count > 0)
                {
                    IntPtr hWnd = IntPtr.Zero;
                    foreach (IntPtr handle in listChild)
                    {
                        System.Windows.Forms.Control tmpHandle = System.Windows.Forms.Control.FromHandle(handle);

                        if (tmpHandle is System.Windows.Forms.DataGridView)
                        {
                            dataGridWorkItem = (System.Windows.Forms.DataGridView)System.Windows.Forms.DataGridView.FromHandle(handle);
                            break;
                        }
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Pending Changes tab not open");
                }
            }
            return dataGridWorkItem;
        }

        /// <summary>
        /// Get ToolStrip Bar on view Listing Work Item
        /// </summary>
        /// <param name="_appObject">Application Object DTE </param>
        /// <returns>ToolStrip of Work Item Window</returns>
        public static System.Windows.Forms.ToolStrip GetWorkItemToolStripLeft(DTE2 _appObject)
        {
            Windows windowItem = _appObject.Windows;
            windowItem.Parent.ActiveWindow.Activate();
            IntPtr windowIntPtr = Buisness.ManageWindows.GetActiveWindow();
            List<IntPtr> listChild = Buisness.ManageWindows.GetChildWindows(windowIntPtr);
            System.Windows.Forms.ToolStrip toolWorkItemLeft = null;
            
            foreach (IntPtr handle in listChild)
            {
                System.Windows.Forms.Control tmp = System.Windows.Forms.Control.FromHandle(handle);
                //Check if is a control is a ToolStrip and if is the good one
                if (tmp is System.Windows.Forms.ToolStrip)
                {
                    System.Diagnostics.Trace.WriteLine(" ToolStrip Looking for :  " + tmp.Name + " Handle : " + handle);
                    if (tmp.Name == "toolStripChannelSelector")
                    {
                        toolWorkItemLeft = (System.Windows.Forms.ToolStrip)System.Windows.Forms.ToolStrip.FromHandle(handle);
                     //   break;
                    }
                }
            }
            return toolWorkItemLeft;
        }

        /// <summary>
        /// Get Work Item ToolStrip Bar which one permit to perform Refresh ...
        /// </summary>
        /// <param name="_appObject">DTE Application Object</param>
        /// <returns>ToolStrip</returns>
        public static System.Windows.Forms.ToolStrip GetWorkItemToolStripTop(DTE2 _appObject)
        {
            Windows windowItem = _appObject.Windows;
            windowItem.Parent.ActiveWindow.Activate();
            IntPtr windowIntPtr = Buisness.ManageWindows.GetActiveWindow();
            List<IntPtr> listChild = Buisness.ManageWindows.GetChildWindows(windowIntPtr);
            System.Windows.Forms.ToolStrip toolWorkItemTop = null;
            foreach (IntPtr handle in listChild)
            {
                System.Windows.Forms.Control tmp = System.Windows.Forms.Control.FromHandle(handle);
                if (tmp is System.Windows.Forms.ToolStrip)
                {
                    
                    if (tmp.Name == "ctlWorkItemsToolstrip")
                    {
                        toolWorkItemTop = (System.Windows.Forms.ToolStrip)System.Windows.Forms.ToolStrip.FromHandle(handle);
                        System.Windows.Forms.Control ctrl = System.Windows.Forms.Control.FromHandle(handle);
                        System.Windows.Forms.Panel panelWorkItem = (System.Windows.Forms.Panel)System.Windows.Forms.Panel.FromHandle(ctrl.Parent.Handle);
                        panelWorkItem.Show();
                       // panelWorkItem.GetChildAtPoint(System.Drawing.Point = new System.Drawing.Point())
                    }
                }
                else
                {
                    int handleHEX = handle.ToInt32();
                    System.Diagnostics.Trace.WriteLine("Handle : " + handleHEX.ToString("X8"));
                }
            }
            return toolWorkItemTop;
        }


    }
}
