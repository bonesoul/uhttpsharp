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

using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace uhttpsharp.Embedded
{
    public sealed class HttpClient
    {
        private readonly TcpClient _client;
        private readonly Stream _inputStream;
        private readonly StreamWriter _outputStream;

        public HttpClient(TcpClient client)
        {
            _client = client;
            _inputStream = new BufferedStream(_client.GetStream());
            _outputStream = new StreamWriter(_client.GetStream());

            var clientThread = new Thread(Process) {IsBackground = true};
            clientThread.Start();
        }

        private void Process()
        {
            while (_client.Connected)
            {
                var request = new HttpRequest(_inputStream);
                if (request.Valid)
                {
                    var response = HttpRequestProxy.Instance.Route(request);
                    if (response != null)
                    {
                        _outputStream.Write(response.Response);
                        _outputStream.Flush();
                        if (response.CloseConnection) _client.Close();
                    }
                }
            }
        }
    }
}