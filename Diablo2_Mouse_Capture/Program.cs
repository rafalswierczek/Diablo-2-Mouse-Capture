using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace Diablo2_Mouse_Capture
{
    public class Program
    {
        public static List<IntPtr> d2Windows = new List<IntPtr>();
        private static DbContext db;

        [DllImport("kernel32.dll")]
        private static extern uint GetProcessId(IntPtr handle);

        static void Main(string[] args)
        {
            db = new DbContext();
            db.CreateTable();

            try
            {
                var startInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    Arguments = "-w -skiptobnet",
                    FileName = "Diablo II.exe",
                    WindowStyle = ProcessWindowStyle.Normal
                };
                Process newD2Process = Process.Start(startInfo);

                var d2HandlersDB = db.FetchData();
                Thread.Sleep(500); // Wait for Diablo II window do display and then run GetDiablo2Windows
                var d2Handlers = WindowsMonitor.GetDiablo2Windows();
                foreach (IntPtr hwnd in d2Handlers)
                {
                    if(!d2HandlersDB.Contains(hwnd))
                    {
                        db.InsertData(hwnd);
                        break;
                    }
                }
            }
            catch(Exception e)
            {
                File.AppendAllText("D2MC_log.log", $"{DateTime.Now} | Main() | {e.Message}{Environment.NewLine}");
                Process.GetCurrentProcess().Kill();
            }

            // issue: If two processes start at the same time, they may both see two active processes and self-terminate. – A.T.
            // https://stackoverflow.com/questions/19147/what-is-the-correct-way-to-create-a-single-instance-wpf-application#answer-41390636
            var currentProcess = Process.GetCurrentProcess();
            var D2MCProcesses = Process.GetProcessesByName(currentProcess.ProcessName);
            if(D2MCProcesses.Count() > 1)
            {
                currentProcess.Kill();
            }
            
            Thread remover = new Thread(new ThreadStart(WindowsMonitor.WindowClosedHandler));
            remover.Start();

            Thread keyHook = new Thread(new ThreadStart(KeyboardHook.Hook));
            keyHook.Start();

            Thread AppCloser = new Thread(new ThreadStart(Program.AppCloser));
            AppCloser.Start();

            while (true)
            {
                MouseActions.CaptureMouseInForegroundD2Win();
                Thread.Sleep(300);
            }
        }

        public static void AppCloser()
        {
            while (true)
            {
                var d2Windows = WindowsMonitor.GetDiablo2Windows();
                if(!d2Windows.Any())
                {
                    db.DeleteAllData();
                    var currentProcess = Process.GetCurrentProcess();
                    currentProcess.Kill();
                }

                Thread.Sleep(2000);
            }
        }
    }
}
