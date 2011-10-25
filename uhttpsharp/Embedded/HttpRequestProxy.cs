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
using System.Collections.Generic;
using System.Reflection;

namespace uhttpsharp.Embedded
{
    internal sealed class HttpRequestProxy
    {
        #region Instance

        private static HttpRequestProxy _instance = new HttpRequestProxy();
        public static HttpRequestProxy Instance { get { return _instance; } }

        #endregion

        private Dictionary<string, HttpRequestHandler> _handlers = new Dictionary<string, HttpRequestHandler>();

        private HttpRequestProxy() 
        {
            this.RegisterHandlers();
        }

        public HttpResponse Route(HttpRequest request)
        {
            HttpResponse response = new HttpResponse(HttpResponse.ResponseCode.OK, string.Format("<html><head><title>{0}</title></head><body><h1>Out of the way you nobgoblin! (404)</h1><hr><b>{0}</b></body></html>", HttpServer.Instance.Banner));
            if (request.Parameters.Function == string.Empty) response = new HttpResponse(HttpResponse.ResponseCode.OK, string.Format("<html><head><title>{0}</title></head><body><h1>Ah, potential customer!</h1><hr><b>{0}</b></body></html>", HttpServer.Instance.Banner));

            foreach (KeyValuePair<string, HttpRequestHandler> pair in this._handlers)
            {
                if (pair.Key == request.Parameters.Function)
                {
                    HttpResponse proxyResponse = pair.Value.Handle(request);
                    if (proxyResponse != null)
                    {
                        response = proxyResponse;
                        break;
                    }
                }
            }

            return response;
        }

        private void RegisterHandlers()
        {
            foreach (Type t in Assembly.GetEntryAssembly().GetTypes())
            {
                if (t.IsSubclassOf(typeof(HttpRequestHandler)))
                {
                    try
                    {
                        object[] attributes = t.GetCustomAttributes(typeof(HttpRequestHandlerAttributes), true);
                        if (attributes.Length > 0)
                        {
                            var handler = (HttpRequestHandler)Activator.CreateInstance(t);
                            this._handlers.Add((attributes[0] as HttpRequestHandlerAttributes).Function, handler);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(string.Format("Exception during activating the IHttpRequestHandler: {0} - {1}", t.ToString(), e));
                    }
                }
            }
        }
    }
}
