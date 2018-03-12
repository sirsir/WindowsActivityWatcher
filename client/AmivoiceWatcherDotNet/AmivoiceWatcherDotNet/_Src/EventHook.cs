using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using System.Management;

#pragma warning disable 1591
// ReSharper disable InconsistentNaming

namespace AmivoiceWatcher
{
    public class EventHook
    {
        public delegate void WinEventDelegate(
              IntPtr hWinEventHook, uint eventType,
              IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(
              uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc,
              uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public const uint EVENT_SYSTEM_FOREGROUND = 3;
        public const uint WINEVENT_OUTOFCONTEXT = 0;
        public const uint EVENT_OBJECT_CREATE = 0x8000;

        private static Process[] processAll;

        readonly IntPtr _hWinEventHook;

       

        public static bool IsOnDesktop()
        {           
            IntPtr cWin = GetForegroundWindow();

            foreach (Process x in processAll)
            {
                if (x.MainWindowHandle == cWin)
                {
                    return false;
                }
            }

            return true;
        }

        public static string GetActiveWindowTitleOld()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }


        public static string GetActiveWindowTitle()
        {
            // SHOULD be define here once to avoid duplicate task
            processAll = Process.GetProcesses();

            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                foreach (Process x in processAll)
                {
                    if (x.MainWindowHandle == handle && !String.IsNullOrEmpty(x.MainWindowTitle))
                    {
                        return x.MainWindowTitle;
                    }
                }
                // Window Explorer sometimes gives empty title if use above code
                return Buff.ToString();
            }

            
            
            return null;
        }

        public static string[] GetActiveWindowProcessNameExe()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                uint pid = 0;
                GetWindowThreadProcessId(handle, out pid);

                string fileName = "";
                string name = "";
                Process p = Process.GetProcessById((int)pid);

                var processname = p.ProcessName;

                switch (processname)
                {
                    case "explorer":
                        return new string[] { "Window Explorer", processname };
                    case "WWAHost":
                        name = GetTitle(handle);
                        return new string[] { name, processname };
                    default:
                        break;
                }
                string wmiQuery = string.Format("SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId LIKE '{0}'", pid.ToString());

                var managementObjCol = (new ManagementObjectSearcher(wmiQuery).Get());

                


                foreach (ManagementObject mo in managementObjCol)
                {
                    try
                    {
                        //if (String.IsNullOrEmpty(mo.ToString()))
                        //{
                        //    break;
                        //}

                        if ( String.IsNullOrEmpty(mo["ExecutablePath"].ToString())){
                            break;
                        }
                        fileName = mo["ExecutablePath"].ToString();// what other fields are there other than name
                                                                   //_osVesion = mo["version"].ToString();
                                                                   //_loginName = mo["csname"].ToString();
                        break;
                    }
                    catch
                    {
                        //Globals.log.Error("Cant get");
                    }
                    
                }


                if (String.IsNullOrEmpty(fileName))
                {
                    return new string[] { processname, processname };
                }

                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(fileName);
                // Get the file description
                name = myFileVersionInfo.FileDescription;
                if (name == "")
                    name = GetTitle(handle);

                return new string[] { name, processname };

            }
            return new string[] { null, null };
        }

        private static string GetTitle(IntPtr handle)
        {
            string windowText = "";
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                windowText = Buff.ToString();
            }
            return windowText;
        }


        //put to caller file
        //static void WinEventProc(IntPtr hWinEventHook, uint eventType,
        //IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        //{
        //    Console.WriteLine("Foreground changed to {0:x8}", hwnd.ToInt32());
        //    Console.WriteLine("Foreground changed to {0:x8}", GetActiveWindowTitle());
        //}

        public EventHook(WinEventDelegate handler, uint eventMin, uint eventMax)
        {
            //_procDelegate = handler;
            
            _hWinEventHook = SetWinEventHook(eventMin, eventMax, IntPtr.Zero, handler, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        public EventHook(WinEventDelegate handler, uint eventMin)
              : this(handler, eventMin, eventMin)
        {
        }

        public void Stop()
        {
            UnhookWinEvent(_hWinEventHook);
        }

        // Usage Example for EVENT_OBJECT_CREATE (http://msdn.microsoft.com/en-us/library/windows/desktop/dd318066%28v=vs.85%29.aspx)
        // var _objectCreateHook = new EventHook(OnObjectCreate, EventHook.EVENT_OBJECT_CREATE);
        // ...
        // static void OnObjectCreate(IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
        //    if (!Win32.GetClassName(hWnd).StartsWith("ClassICareAbout"))
        //        return;
        // Note - in Console program, doesn't fire if you have a Console.ReadLine active, so use a Form
    }
}
