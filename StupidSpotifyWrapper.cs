using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketCshart
{
    internal class StupidSpotifyWrapper
    {
        static int process;
        public static string GetOther(string processListen)
        {
            var Processs = Process.GetProcessesByName(processListen).FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
            if (Processs == null)
            {
                return "Process Closed";
            }
            return Processs.MainWindowTitle;
        }
        public static string SpotifySend()
        {
            if (process == 0 || Process.GetProcessById(process) == null)
            {
                process = GetSpotifyPid();
                Console.WriteLine($"process collected: {process}, dont close spotify >:(");
            }
            string window = Process.GetProcessById(process).MainWindowTitle;
            if (window != null)
            {
                string result = "fuck you";
                if (window == "Advertisement" || window == "Spotify Free" || window == "Spotify Premium")
                    result = $"Music Paused";
                else
                    result = window;

                if (result.Contains("("))
                {
                    string[] subs = result.Split("(");
                     
                    result = subs[0];
                }
                return result;
            }
            return null;
        }
        private static int GetSpotifyPid()
        {
            foreach (Process p in Process.GetProcesses())
                if (p.ProcessName == "Spotify")
                {
                    return p.Id;
                }
            return 0;
        }
    }
}
