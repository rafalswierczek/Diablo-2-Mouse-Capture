using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;
using System.Management;
using System.Threading;

namespace Diablo2_Mouse_Capture
{
    class Program
    {
        static void Main(string[] args)
        {
            Asd asd = new Asd();
            while(true)
            {
                foreach (int window in asd.GetDiablo2Windows())
                {
                    if(window == asd.GetActiveWindow())
                    {
                        asd.CaptureMouse(window);
                    }
                }
                Thread.Sleep(330);
            }
        }
    }

    public class Asd
    {
        private Rectangle winSizes = new Rectangle();

        #region ImportDLLs
        // int hwnd works only with 32-bit apps (Diablo 2) // IntPtr works with 32-bit and 64-bit apps
        [DllImport("user32.dll")]
        private static extern int GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        public delegate bool EnumWindowsProc(int hwnd, int lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(int hwnd, StringBuilder lpString, int lpStringLength);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(int hwnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowRect(int hwnd, ref Rectangle lpRect);

        [DllImport("user32.dll")]
        private static extern int ClipCursor(ref Rectangle lpRect);

        //[DllImport("user32.dll")]
        //private static extern WindowStyles GetWindowLong(int hwnd, GetWindowLongIndex index);

        //[DllImport("user32.dll")]
        //private static extern int GetSystemMetrics(SystemMetric index);
        #endregion

        /*#region EnumerationsAndFlags
        private enum GetWindowLongIndex : int
        {
            GWL_WNDPROC = -4, GWL_HINSTANCE = -6, GWL_HWNDPARENT = -8, GWL_STYLE = -16, GWL_EXSTYLE = -20, GWL_USERDATA = -21, GWL_ID = -12
        }

        [Flags]
        private enum WindowStyles : int
        {
            WS_OVERLAPPED = 0x00000000, WS_POPUP = -2147483648, WS_CHILD = 0x40000000, WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000, WS_DISABLED = 0x08000000, WS_CLIPSIBLINGS = 0x04000000, WS_CLIPCHILDREN = 0x02000000,
            WS_MAXIMIZE = 0x01000000, WS_CAPTION = 0x00C00000, WS_BORDER = 0x00800000, WS_DLGFRAME = 0x00400000,
            WS_VSCROLL = 0x00200000, WS_HSCROLL = 0x00100000, WS_SYSMENU = 0x00080000, WS_THICKFRAME = 0x00040000,
            WS_GROUP = 0x00020000, WS_TABSTOP = 0x00010000, WS_MINIMIZEBOX = 0x00020000, WS_MAXIMIZEBOX = 0x00010000
        }

        private enum SystemMetric : int
        {
            SM_CXBORDER = 5, SM_CYBORDER = 6, SM_CXSIZEFRAME = 32, SM_CYSIZEFRAME = 33, SM_CYCAPTION = 4, SM_CXFIXEDFRAME = 7, SM_CYFIXEDFRAME = 8
        }
        #endregion*/

        /*public Rectangle GetWindowBorderSizes(int hwnd)
        {
            Rectangle windowBorderSizes = new Rectangle();

            WindowStyles styles = GetWindowLong(hwnd, GetWindowLongIndex.GWL_STYLE);

            // Window has title-bar
            if (styles.HasFlag(WindowStyles.WS_CAPTION))
            {
                windowBorderSizes.Top += GetSystemMetrics(SystemMetric.SM_CYCAPTION);
            }

            // Window has re-sizable borders
            if (styles.HasFlag(WindowStyles.WS_THICKFRAME))
            {
                windowBorderSizes.Left += GetSystemMetrics(SystemMetric.SM_CXSIZEFRAME);
                windowBorderSizes.Right += GetSystemMetrics(SystemMetric.SM_CXSIZEFRAME);
                windowBorderSizes.Top += GetSystemMetrics(SystemMetric.SM_CYSIZEFRAME);
                windowBorderSizes.Bottom += GetSystemMetrics(SystemMetric.SM_CYSIZEFRAME);
            }
            else if (styles.HasFlag(WindowStyles.WS_BORDER) || styles.HasFlag(WindowStyles.WS_CAPTION))
            {
                // Window has normal borders
                windowBorderSizes.Left += GetSystemMetrics(SystemMetric.SM_CXFIXEDFRAME);
                windowBorderSizes.Right += GetSystemMetrics(SystemMetric.SM_CXFIXEDFRAME);
                windowBorderSizes.Top += GetSystemMetrics(SystemMetric.SM_CYFIXEDFRAME);
                windowBorderSizes.Bottom += GetSystemMetrics(SystemMetric.SM_CYFIXEDFRAME);
            }

            return windowBorderSizes;
        }*/

        public struct Rectangle
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public override string ToString()
            {
                return string.Format("Left : {0:d}, Top : {1:d}, Right : {2:d}, Bottom : {3:d}", Left, Top, Right, Bottom);
            }
        }

        public Rectangle GetWindowSizes(int hwnd)
        {
            GetWindowRect(hwnd, ref winSizes);
            return winSizes;
        }

        public int GetActiveWindow()
        {
            return GetForegroundWindow();
        }

        public void CaptureMouse(int hwnd)
        {
            GetWindowRect(hwnd, ref winSizes);
            //Rectangle windowBorders = GetWindowBorderSizes(hwnd);

            winSizes.Left += 3;//windowBorders.Left;
            winSizes.Right -= 3;//windowBorders.Right;
            winSizes.Top += 26;//windowBorders.Top;
            winSizes.Bottom -= 3;//windowBorders.Bottom;

            ClipCursor(ref winSizes);
        }

        public IEnumerable<int> GetDiablo2Windows()
        {
            return FindWindows(delegate (int hwnd, int param)
            {
                return GetWindowTitle(hwnd).Contains("Diablo II");
            });
        }
        public IEnumerable<int> FindWindows(EnumWindowsProc filter)
        {
            List<int> windows = new List<int>();

            EnumWindows(delegate (int hwnd, int param)
            {
                if (filter(hwnd, param))
                {
                    windows.Add(hwnd);
                }
                return true; // enum true => next iteration (all windows iterated)
            }, IntPtr.Zero);

            return windows;
        }
        public static string GetWindowTitle(int hwnd)
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
