using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace D2MC
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

            try
            {
                Process.Start("D2MC.bat");
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

            Thread AppCloser = new Thread(new ThreadStart(Program.AppCloser));
            AppCloser.Start();

            Thread remover = new Thread(new ThreadStart(WindowsMonitor.WindowChangeHandler));
            remover.Start();

            Thread keyHook = new Thread(new ThreadStart(KeyboardHook.Hook));
            keyHook.Start();

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
                Thread.Sleep(10000);

                var d2Windows = WindowsMonitor.GetDiablo2Windows();
                if(!d2Windows.Any())
                {
                    db.DeleteAllData();
                    var currentProcess = Process.GetCurrentProcess();
                    currentProcess.Kill();
                }
            }
        }
    }
}
