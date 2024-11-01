﻿using System;
using System.Net;
using System.Net.Sockets;

namespace SocketCshart
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AlignBytes.Startup();
            string authCode = await Auth.GetAuthorizationCodeAsync();
            if (!string.IsNullOrEmpty(authCode)) {
                await Auth.InitializeTokensAsync(authCode);
            while(true) {
                await VRChatFunctions.ToastString(2000);
                }
            }
        }
    }
}