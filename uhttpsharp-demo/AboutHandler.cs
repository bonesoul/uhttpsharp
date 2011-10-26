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

using uhttpsharp.Embedded;

namespace uhttpsharpdemo
{
    [HttpRequestHandlerAttributes("about")]
    public class AboutHandler : HttpRequestHandler
    {
        public override HttpResponse Handle(HttpRequest httpRequest)
        {
            return new HttpResponse(HttpResponse.ResponseCode.Ok,
                                    string.Format(
                                        "<html><head><title>uhttpsharp</title></head><body><h1>A sample http-request-handler!</h1><hr><b>{0}</b></body></html>",
                                        HttpServer.Instance.Banner));
        }
    }
}