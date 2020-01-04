using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace D2MC
{
    public static class KeyboardHook
    {
        public static bool ctrlPressed;
        public static bool ePressed;
        public static bool mouseFree = false;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        #region ImportDLLs
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int SetWindowPos(IntPtr hwnd, int hwndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hwnd, uint nCmdShow);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int HWND_TOPMOST = -1;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        private const uint SW_MINIMIZE = 6;
        private const uint SW_NORMAL = 1;

        public static void Hook()
        {
            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID); // never used ?
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                var d2Windows = Program.d2Windows;
                if (vkCode == 162)
                {
                    ctrlPressed = true;
                }
                else if (vkCode == 69)
                {
                    ePressed = true;
                }
                else if (vkCode == 35 || vkCode == 40 || vkCode == 34 || vkCode == 37 || vkCode == 12 || vkCode == 39 || vkCode == 36 || vkCode == 38)
                {
                    switch (vkCode)
                    {
                        case 35:
                            if (d2Windows.Count > 0)
                            {
                                ShowWindowAsync(d2Windows[0], SW_NORMAL);
                                SetWindowPos(d2Windows[0], HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
                            }
                            break;
                        case 40:
                            if (d2Windows.Count > 1)
                            {
                                ShowWindowAsync(d2Windows[1], SW_NORMAL);
                                SetWindowPos(d2Windows[1], HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
                            }
                            break;
                        case 34:
                            if (d2Windows.Count > 2)
                            {
                                ShowWindowAsync(d2Windows[2], SW_NORMAL);
                                SetWindowPos(d2Windows[2], HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
                            }
                            break;
                        case 37:
                            if (d2Windows.Count > 3)
                            {
                                ShowWindowAsync(d2Windows[3], SW_NORMAL);
                                SetWindowPos(d2Windows[3], HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
                            }
                            break;
                        case 12:
                            if (d2Windows.Count > 4)
                            {
                                ShowWindowAsync(d2Windows[4], SW_NORMAL);
                                SetWindowPos(d2Windows[4], HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
                            }
                            break;
                        case 39:
                            if (d2Windows.Count > 5)
                            {
                                ShowWindowAsync(d2Windows[5], SW_NORMAL);
                                SetWindowPos(d2Windows[5], HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
                            }
                            break;
                        case 36:
                            if (d2Windows.Count > 6)
                            {
                                ShowWindowAsync(d2Windows[6], SW_NORMAL);
                                SetWindowPos(d2Windows[6], HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
                            }
                            break;
                        case 38:
                            if (d2Windows.Count > 7)
                            {
                                ShowWindowAsync(d2Windows[7], SW_NORMAL);
                                SetWindowPos(d2Windows[7], HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
                            }
                            break;
                    }
                }

                if (ctrlPressed && vkCode == 32) // ctrl + space
                {
                    foreach (IntPtr d2Window in d2Windows)
                    {
                        ShowWindowAsync(d2Window, SW_MINIMIZE);
                    }
                }

                if (ctrlPressed && ePressed)
                {
                    IntPtr capturedDiablo2Window = IntPtr.Zero;
                    foreach (IntPtr window in d2Windows)
                    {
                        if (window == GetForegroundWindow())
                        {
                            capturedDiablo2Window = window;
                            break;
                        }
                    }
                    if (!mouseFree && capturedDiablo2Window != IntPtr.Zero)
                    {
                        MouseActions.winSizes.Left = 0;
                        MouseActions.winSizes.Right = SystemInformation.VirtualScreen.Width;
                        MouseActions.winSizes.Top = 0;
                        MouseActions.winSizes.Bottom = SystemInformation.VirtualScreen.Height;

                        MouseActions.ClipCursor(ref MouseActions.winSizes);
                        mouseFree = true;
                    }
                    else if (mouseFree && capturedDiablo2Window != IntPtr.Zero)
                    {
                        MouseActions.CaptureMouse(capturedDiablo2Window);
                        mouseFree = false;
                    }
                }
            }
            else if (wParam == (IntPtr)WM_KEYUP)
            {
                if (vkCode == 162)
                {
                    ctrlPressed = false;
                }
                else if (vkCode == 69)
                {
                    ePressed = false;
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
