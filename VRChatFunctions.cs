using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SocketCshart
{
    internal class VRChatFunctions
    {
        public static async void ChatBox(string text)
        {
            AlignBytes.Send("/chatbox/input", await Replaces(text), true, false);
        }
        public static async Task ToastString(int delayms)
        {
            var dir = Directory.GetCurrentDirectory() + @"\ToastPackages\YourFavoritePhrases.txt";
            var lines = File.ReadAllLines(dir);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var response = await Replaces(line);
                VRChatFunctions.ChatBox(response);
                Console.WriteLine(response);
                Thread.Sleep(delayms);
            }
        }
        public static async Task<string> Replaces(string input)
        {
            var splits = await Splits();
            input = Regex.Replace(input, "{/v}", "\u2029");
            input = Regex.Replace(input, "{Track}", splits[0]);
            input = Regex.Replace(input, "{Time}", splits[1]);
            return input;
        }
        private static async Task<string[]> Splits()
        {
            string[] arr = [];
            var ret = await StupidSpotifyWrapper.GetCurrentlyPlayingTrack();
            arr=ret.Split("{/v}");
            return arr;
        }
    }
}
