using System;
using System.Runtime.InteropServices;

namespace Diablo2_Mouse_Capture
{
    public static class MouseActions
    {
        public static D2Rect winSizes = new D2Rect();

        #region ImportDLLs
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int ClipCursor(ref D2Rect lpRect);

        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hwnd, ref D2Rect lpRect);

        [DllImport("user32.dll")]
        private static extern WindowStyles GetWindowLong(IntPtr hwnd, GetWindowLongIndex index);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(SystemMetric index);

        #endregion

        #region Flags_Structs
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

        public struct D2Rect
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
        #endregion

        public static void CaptureMouseInForegroundD2Win()
        {
            foreach (IntPtr hwnd in Program.d2Windows)
            {
                if (hwnd == GetForegroundWindow() && !KeyboardHook.mouseFree)
                {
                    GetWindowRect(hwnd, ref winSizes);
                    D2Rect windowBorders = GetWindowBorderSizes(hwnd);

                    winSizes.Left += windowBorders.Left;
                    winSizes.Right -= windowBorders.Right;
                    winSizes.Top += windowBorders.Top;
                    winSizes.Bottom -= windowBorders.Bottom;

                    ClipCursor(ref winSizes);
                }
            }
        }

        public static void CaptureMouse(IntPtr hwnd)
        {
            GetWindowRect(hwnd, ref winSizes);
            D2Rect windowBorders = GetWindowBorderSizes(hwnd);

            winSizes.Left += windowBorders.Left;
            winSizes.Right -= windowBorders.Right;
            winSizes.Top += windowBorders.Top;
            winSizes.Bottom -= windowBorders.Bottom;

            ClipCursor(ref winSizes);
        }

        public static D2Rect GetWindowBorderSizes(IntPtr hwnd)
        {
            D2Rect windowBorderSizes = new D2Rect();

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
        }
    }
}
