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
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Headers;

namespace uhttpsharpdemo.Handlers
{
    public class IndexHandler : IHttpRequestHandler
    {
        private readonly HttpResponse _response;
        private readonly HttpResponse _keepAliveResponse;

        public IndexHandler()
        {
            byte[] contents = Encoding.UTF8.GetBytes("Welcome to the Index.");
            _keepAliveResponse = new HttpResponse(HttpResponseCode.Ok, contents, true);
            _response = new HttpResponse(HttpResponseCode.Ok, contents, false);
        }
        
        public Task Handle(IHttpContext context, Func<Task> next)
        {
            context.Response = context.Request.Headers.KeepAliveConnection() ? _keepAliveResponse : _response;
            return Task.Factory.GetCompleted();
        }
    }
}