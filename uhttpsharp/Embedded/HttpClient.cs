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

using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace uhttpsharp.Embedded
{
    public sealed class HttpClient
    {
        private TcpClient _client;
        private Stream _inputStream;
        private StreamWriter _outputStream;

        public HttpClient(TcpClient client)
        {
            this._client = client;
            this._inputStream = new BufferedStream(this._client.GetStream());
            this._outputStream = new StreamWriter(this._client.GetStream());
                
            Thread clientThread = new Thread(() => { this.Process(); }) { IsBackground = true };
            clientThread.Start();
        }

        private void Process()
        {
            while (this._client.Connected)
            {
                HttpRequest request = new HttpRequest(this._inputStream);
                if (request.Valid)
                {
                    HttpResponse response = HttpRequestProxy.Instance.Route(request);
                    if (response != null)
                    {
                        this._outputStream.Write(response.Response);
                        this._outputStream.Flush();
                        if (response.CloseConnection) this._client.Close();
                    }
                }
            }
        }
    }
}
