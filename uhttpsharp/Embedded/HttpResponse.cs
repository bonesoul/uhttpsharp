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
using System.IO;

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
        private Stream ContentStream { get; set; }

        public HttpResponse(ResponseCode code, string content)
            : this(code, "text/html; charset=utf-8", StringToStream(content))
        {
        }
        public HttpResponse(string contentType, Stream contentStream)
            : this(ResponseCode.Ok, contentType, contentStream)
        {
        }
        private HttpResponse(ResponseCode code, string contentType, Stream contentStream)
        {
            Protocol = "HTTP/1.1";
            ContentType = contentType;
            CloseConnection = false;

            Code = code;
            ContentStream = contentStream;
        }

        private static Stream StringToStream(string content)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            return stream;
        }
        public void WriteResponse(Stream stream)
        {
            var writer = new StreamWriter(stream) {NewLine = "\r\n"};
            writer.WriteLine("{0} {1} {2}", Protocol, (int) Code, _responseTexts[(int) Code]);
            writer.WriteLine("Date: {0}", DateTime.Now.ToString("R"));
            writer.WriteLine("Server: {0}", HttpServer.Instance.Banner);
            writer.WriteLine("Connection: {0}", CloseConnection ? "close" : "Keep-Alive");
            writer.WriteLine("Content-Type: {0}", ContentType);
            writer.WriteLine("Content-Length: {0}", ContentStream.Length);
            writer.WriteLine();
            writer.Flush();

            ContentStream.Position = 0;
            ContentStream.CopyTo(stream);
            ContentStream.Close();
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