using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace D2MC
{
    public static class WindowsMonitor
    {
        #region ImportDLLs
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hwnd, StringBuilder lpString, int lpStringLength);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint lpdwProcessId);

        #endregion

        public static void WindowChangeHandler()
        {
            DbContext db = new DbContext();
            while (true)
            {
                Program.d2Windows = db.FetchData();
                var d2Handlers = GetDiablo2Windows();

                if(d2Handlers.Count() > 0)
                {
                    foreach (IntPtr hwnd in d2Handlers)
                    {
                        if (!Program.d2Windows.Contains(hwnd))
                        {
                            db.InsertData(hwnd);
                            break;
                        }
                    }
                }

                if (Program.d2Windows.Count > 0)
                {
                    foreach (IntPtr hwnd in Program.d2Windows)
                    {
                        if (!IsWindow(hwnd))
                        {
                            db.DeleteData(hwnd);
                        }
                    }
                }

                Thread.Sleep(300);
            }
        }

        public static IEnumerable<IntPtr> GetDiablo2Windows()
        {
            return FindWindows(delegate (IntPtr hwnd, IntPtr param)
            {
                // Very slow approach which excludes other thrash windows with "Diablo II" caption title
                if (GetWindowTitle(hwnd) != "Diablo II") { return false; }
                var a = GetWindowTitle(hwnd);
                GetWindowThreadProcessId(hwnd, out uint processId);
                try
                {
                    var windowProcess = Process.GetProcessById((int)processId);
                    ProcessModule module = windowProcess.MainModule;
                    // if(window title is "Diablo II" && it's actaully type of Diablo 2 game window (e.g. not explorer.exe's folder window with "Diablo II" title)){
                    if (GetWindowTitle(hwnd) == "Diablo II" && module.ModuleName == "Game.exe")
                    {
                        return true; // confirm that current hwnd iteration which represent Diablo 2 game window should be added to IEnumerable<IntPtr> GetDiablo2Windows()
                    }
                    else
                    {
                        return false;
                    } 
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            });
        }
        private static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
        {
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate (IntPtr hwnd, IntPtr param)
            {
                if (filter(hwnd, param))
                {
                    windows.Add(hwnd);
                }
                return true; // enum true => next iteration (all windows iterated)
            }, IntPtr.Zero);

            return windows;
        }
        private static string GetWindowTitle(IntPtr hwnd)
        {
            int size = GetWindowTextLength(hwnd);
            if (size > 0)
            {
                var builder = new StringBuilder(size + 1);
                GetWindowText(hwnd, builder, builder.Capacity);
                return builder.ToString();
            }

            return string.Empty;
        }
    }
}
