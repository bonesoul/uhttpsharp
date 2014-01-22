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

using System.Text;
using log4net;
using System.Net;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using uhttpsharp.Clients;
using uhttpsharp.Headers;
using uhttpsharp.RequestProviders;

namespace uhttpsharp
{
    internal sealed class HttpClient
    {
        private const string CrLf = "\r\n";
        private static readonly byte[] CrLfBuffer = Encoding.UTF8.GetBytes(CrLf);

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly IClient _client;
        private readonly Stream _stream;
        private readonly IList<IHttpRequestHandler> _requestHandlers;
        private readonly IHttpRequestProvider _requestProvider;
        private readonly EndPoint _remoteEndPoint;

        public HttpClient(IClient client, IList<IHttpRequestHandler> requestHandlers, IHttpRequestProvider requestProvider)
        {
            _remoteEndPoint = client.RemoteEndPoint;
            _client = client;
            _requestHandlers = requestHandlers;
            _requestProvider = requestProvider;            

            _stream = new BufferedStream(_client.Stream);

            Logger.InfoFormat("Got Client {0}", _remoteEndPoint);

            Task.Factory.StartNew(Process);
        }

        private async void Process()
        {
            try
            {
                while (_client.Connected)
                {
                    // TODO : Extract read and write limit to configuration.
                    var requestStream = new LimitedStream(_stream, readLimit: 1024*1024, writeLimit: 1024*1024);
                    var request = await _requestProvider.Provide(new StreamReader(requestStream)).ConfigureAwait(false);

                    if (request != null)
                    {

                        var context = new HttpContext(request);

                        Logger.InfoFormat("{1} : Got request {0}", request.Uri, _client.RemoteEndPoint);

                        var getResponse = BuildHandlers(context)();

                        await getResponse.ConfigureAwait(false);

                        var response = context.Response;

                        if (response != null)
                        {
                            var streamWriter = new StreamWriter(requestStream);
                            // Headers
                            await response.WriteHeaders(streamWriter).ConfigureAwait(false);

                            // Cookies
                            if (context.Cookies.Touched)
                            {
                                await streamWriter.WriteAsync(context.Cookies.ToCookieData())
                                    .ConfigureAwait(false);
                                await streamWriter.FlushAsync().ConfigureAwait(false);
                            }

                            // Empty Line
                            await requestStream.WriteAsync(CrLfBuffer, 0, CrLfBuffer.Length).ConfigureAwait(false);

                            // Body
                            await response.WriteResponse(streamWriter).ConfigureAwait(false);

                            if (!request.Headers.KeepAliveConnection() || response.CloseConnection)
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
                // ALSO : LimitedStream throws IOException on limit exceed.
            }
            catch (Exception e)
            {
                // Hate people who make bad calls.
                Logger.Warn(string.Format("Error while serving : {0} : {1}{2}", _remoteEndPoint, Environment.NewLine, e));
                _client.Close();
            }

            Logger.InfoFormat("Lost Client {0}", _remoteEndPoint);
        }


        private Func<Task> BuildHandlers(IHttpContext context, int index = 0)
        {
            if (index > _requestHandlers.Count)
            {
                return null;
            }

            return () => _requestHandlers[index].Handle(context, BuildHandlers(context, index + 1));
        }
    }
}