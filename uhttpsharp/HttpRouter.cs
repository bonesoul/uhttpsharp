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
using System.Threading.Tasks;

namespace uhttpsharp
{
    public class HttpRouter : IHttpRequestHandler
    {
        private readonly IDictionary<string, IHttpRequestHandler> _handlers = new Dictionary<string, IHttpRequestHandler>();

        public HttpRouter With(string function, IHttpRequestHandler handler)
        {
            _handlers.Add(function, handler);

            return this;
        }

        public Task<HttpResponse> Handle(HttpRequest request, Func<Task<HttpResponse>> nextHandler)
        {
            var function = request.Parameters.Function;

            IHttpRequestHandler value;
            if (_handlers.TryGetValue(function, out value))
            {
                return value.Handle(request, nextHandler);
            }

            // Route not found, Call next.
            return nextHandler();
        }
    }
}