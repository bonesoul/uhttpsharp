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
using System.Threading;
using System.Reflection;

namespace uhttpsharp.Embedded
{
    public sealed class HttpServer
    {
        #region Instance

        private static HttpServer _instance = new HttpServer();
        public static HttpServer Instance { get { return _instance; } }

        #endregion

        public int Port = 80;
        public string Address = string.Empty;
        public string Banner = string.Empty;
        

        private TcpListener _listener;
        private bool _isActive = false;

        private HttpServer() 
        {
            this.Address = string.Format("127.0.0.1:{0}", this.Port);
            this.Banner = string.Format("uhttpsharp {0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        public void StartUp()
        {
            Thread serverThread = new Thread(() => { this.Listen(); }) { IsBackground = true };
            serverThread.Start();
        }

        private void Listen()
        {
            this._isActive = true;
            this._listener = new TcpListener(IPAddress.Loopback, this.Port);
            this._listener.Start();

            Console.WriteLine(string.Format("Embedded httpserver started.. [{0}:{1}]", IPAddress.Loopback, this.Port));

            while (this._isActive)
            {
                HttpClient httpClient = new HttpClient(this._listener.AcceptTcpClient());
            }
        }
    }
}
