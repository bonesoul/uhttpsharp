/*
 * Copyright (C) 2011 uhttpsharp project - http://github.com/raistlinthewiz/uhttpsharp
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.

 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace uhttpsharp.Embedded
{
    public sealed class HttpServer
    {
        #region Instance
        private static readonly HttpServer _instance = new HttpServer();
        public static HttpServer Instance
        {
            get { return _instance; }
        }
        #endregion

        public int Port = 80;
        public string Address = string.Empty;
        public string Banner = string.Empty;

        private TcpListener _listener;
        private bool _isActive;

        private HttpServer()
        {
            Address = string.Format("127.0.0.1:{0}", Port);
            Banner = string.Format("uhttpsharp {0}", Assembly.GetExecutingAssembly().GetName().Version);
        }

        public void StartUp()
        {
            var serverThread = new Thread(Listen) {IsBackground = true};
            serverThread.Start();
        }

        private void Listen()
        {
            _isActive = true;
            _listener = new TcpListener(IPAddress.Loopback, Port);
            _listener.Start();

            Console.WriteLine(string.Format("Embedded httpserver started.. [{0}:{1}]", IPAddress.Loopback, Port));

            while (_isActive)
            {
                new HttpClient(_listener.AcceptTcpClient());
            }
        }
    }
}