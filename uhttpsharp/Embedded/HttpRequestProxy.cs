/*  
 * uhttpsharp - A very lightweight & simple embedded http server for c# - http://code.google.com/p/uhttpsharp/
 * 
 * Copyright (c) 2010, Hüseyin Uslu
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *  
 *   Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * 
 *   Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer 
 *   in the documentation and/or other materials provided with the distribution.
 *   
 *   Neither the name of the uhttpsharp nor the names of its contributors may be used to endorse or promote products derived from 
 *   this software without specific prior written permission.
 *   
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT 
 * NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
 * THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE 
 * OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
