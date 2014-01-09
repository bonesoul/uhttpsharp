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

using System.Net;
using System.Reflection;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace uhttpsharp
{
    internal sealed class HttpClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly TcpClient _client;
        private readonly StreamReader _inputStream;
        private readonly StreamWriter _outputStream;
        private readonly IList<IHttpRequestHandler> _requestHandlers;
        private readonly IHttpRequestProvider _requestProvider;
        private readonly EndPoint _remoteEndPoint;

        public HttpClient(TcpClient client, IList<IHttpRequestHandler> requestHandlers, IHttpRequestProvider requestProvider)
        {
            _remoteEndPoint = client.Client.RemoteEndPoint;
            _client = client;
            _requestHandlers = requestHandlers;
            _requestProvider = requestProvider;
            _outputStream = new StreamWriter(new BufferedStream(client.GetStream())) {NewLine = "\r\n"};
            _inputStream = new StreamReader(new BufferedStream(_client.GetStream()));

            Logger.InfoFormat("Got Client {0}", _remoteEndPoint);

            Task.Factory.StartNew(Process);
        }

        private async void Process()
        {
            try
            {
                while (_client.Connected)
                {
                    var request = await _requestProvider.Provide(_inputStream).ConfigureAwait(false);

                    if (request != null)
                    {

                        Logger.InfoFormat("{1} : Got request {0}", request.Uri, _client.Client.RemoteEndPoint);

                        var getResponse = BuildHandlers(request)();

                        var response = await getResponse.ConfigureAwait(false);

                        if (response != null)
                        {
                            await response.WriteResponse(_outputStream).ConfigureAwait(false);
                            if (response.CloseConnection)
                            {
                                _client.Close();
                            }
                        }
                    }
                    else
                    {
                        _client.Close();
                    }
                }
            }
            catch (SocketException)
            {
            }
            catch (IOException)
            {
                // Socket exceptions on read will be re-thrown as IOException by BufferedStream
            }

            Logger.InfoFormat("Lost Client {0}", _remoteEndPoint);
        }


        private Func<Task<HttpResponse>> BuildHandlers(IHttpRequest request, int index = 0)
        {
            if (index > _requestHandlers.Count)
            {
                return null;
            }

            return () => _requestHandlers[index].Handle(request, BuildHandlers(request, index + 1));
        }
    }
}