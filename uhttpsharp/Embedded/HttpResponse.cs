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

namespace uhttpsharp.Embedded
{
    public sealed class HttpResponse
    {
        private readonly Dictionary<int, string> _responseTexts =
            new Dictionary<int, string>
                {
                    {200, "OK"},
                    {302, "Found"},
                    {303, "See Other"},
                    {400, "BadRequest"},
                    {404, "Not Found"},
                    {502, "Server Busy"},
                    {500, "Internal Server Error"}
                };

        public string Protocol { get; private set; }
        public string ContentType { get; private set; }
        public bool CloseConnection { get; private set; }
        public ResponseCode Code { get; private set; }
        public string Content { get; private set; }
        public string Response { get; private set; }

        public HttpResponse(ResponseCode code, string content)
        {
            Protocol = "HTTP/1.1";
            ContentType = "text/html";
            CloseConnection = true;

            Code = code;
            Content = content;
            ForgeResponse();
        }

        private void ForgeResponse()
        {
            Response =
                string.Format(
                    "{0} {1} {2}\r\nDate: {3}\r\nServer: {4}\r\nConnection: {5}\r\nContent-Type: {6}\r\nContent-Length: {7}\r\n\r\n{8}",
                    Protocol,
                    (int)Code,
                    _responseTexts[(int)Code],
                    DateTime.Now.ToString("R"),
                    HttpServer.Instance.Banner,
                    CloseConnection ? "close" : "Keep-Alive",
                    ContentType,
                    Content.Length,
                    Content);
        }

        public enum ResponseCode
        {
            Ok = 200,
            Found = 302,
            SeeOther = 303,
            BadRequest = 400,
            NotFound = 404,
            InternalServerError = 500,
            ServerBusy = 502,
        }
    }
}