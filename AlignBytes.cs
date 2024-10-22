using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketCshart
{
    internal class AlignBytes
    {
        static List<object> _arguments;
        public static Socket _sock;
        public static IPEndPoint _remoteIpEndPoint;
        public static void Send(string address, params object[] args)
        {
            _arguments = new List<object>();
            _arguments.AddRange(args);
            _sock.SendTo(GetBytes(address), _remoteIpEndPoint);
        }
        private static byte[] GetBytes(string address)
        {
            var parts = new List<byte[]>();
            var typeString = ",";

            var currentList = _arguments;
            var argumentsIndex = 0;

            var i = 0;
            while (i < currentList.Count)
            {
                var arg = currentList[i];
                switch (arg.GetType().ToString())
                {
                    case "System.Int32":
                        typeString += "i";
                        parts.Add(SetInt((int)arg));
                        break;

                    case "System.Single":
                        typeString += "f";
                        parts.Add(SetFloat((float)arg));
                        break;

                    case "System.Boolean":
                        typeString += (bool)arg ? "T" : "F";
                        break;

                    case "System.String":
                        typeString += "s";
                        parts.Add(SetString((string)arg));
                        break;
                }

                i++;
                if (currentList == _arguments || i != currentList.Count) continue;
                typeString += "]";
                currentList = _arguments;
                i = argumentsIndex + 1;
            }

            typeString += "]";
            var addressLen = AlignedStringLength(address);
            var typeLen = AlignedStringLength(typeString);
            var total = addressLen + typeLen + parts.Sum(x => x.Length);
            var output = new byte[total];
            i = 0;

            Encoding.ASCII.GetBytes(address).CopyTo(output, i);
            i += addressLen;

            Encoding.ASCII.GetBytes(typeString).CopyTo(output, i);
            i += typeLen;

            foreach (var b in parts)
            {
                b.CopyTo(output, i);
                i += b.Length;
            }
            return output;
        }
        private static int AlignedStringLength(string val)
        {
            var len = val.Length + (4 - val.Length % 4);
            if (len <= val.Length) len += 4;

            return len;
        }

        private static byte[] SetInt(int value)
        {
            var msg = new byte[4];

            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, msg, Math.Min(msg.Length, bytes.Length));

            return msg;
        }

        private static byte[] SetFloat(float value)
        {
            var msg = new byte[4];

            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, msg, Math.Min(msg.Length, bytes.Length));

            return msg;
        }

        private static byte[] SetString(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);

            var msg = new byte[(bytes.Length / 4 + 1) * 4];
            bytes.CopyTo(msg, 0);

            return msg;
        }
    }
}
