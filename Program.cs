using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;

namespace SocketCshart
{
    class Program
    {
        static string serverIP = "127.0.0.1";
        static int serverPort = 9000;
        static void Main(string[] args)
        {
            string process;
            AlignBytes._sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var addresses = Dns.GetHostAddresses(serverIP);
            AlignBytes._remoteIpEndPoint = new IPEndPoint(addresses[0], serverPort);
            Console.WriteLine("enter the desired delay between sends");
            var delay = Console.ReadLine();
            Console.WriteLine("enter the name of the process you would like to collect");
            process = Console.ReadLine();
            while (true)
            {
                if (int.Parse(delay)== 0)
                {
                    VRChatFunctions.OnNewSong();
                }
                else
                    VRChatFunctions.ToastString(process, int.Parse(delay));
            }
        }
    }
}