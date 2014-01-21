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
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using uhttpsharp;

namespace uhttpsharp
{
    public interface IHttpResponse
    {
        Task WriteResponse(StreamWriter writer);

        bool CloseConnection { get; }
    }

    public sealed class HttpResponse : IHttpResponse
    {
        private static readonly Dictionary<int, string> ResponseTexts =
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
        public HttpResponseCode Code { get; private set; }
        private Stream ContentStream { get; set; }

        private readonly Stream _headerStream = new MemoryStream();
        private readonly bool _closeConnection;

        public HttpResponse(HttpResponseCode code, string content, bool closeConnection)
            : this(code, "text/html; charset=utf-8", StringToStream(content), closeConnection)
        {
        }
        public HttpResponse(string contentType, Stream contentStream, bool closeConnection)
            : this(HttpResponseCode.Ok, contentType, contentStream, closeConnection)
        {
        }
        public HttpResponse(HttpResponseCode code, string contentType, Stream contentStream, bool keepAliveConnection)
        {
            Protocol = "HTTP/1.1";
            ContentType = contentType;
            
            Code = code;
            ContentStream = contentStream;
            _closeConnection = !keepAliveConnection;

            WriteHeaders(new StreamWriter(_headerStream));
        }
        public HttpResponse(HttpResponseCode code, byte[] contentStream, bool closeConnection) 
            : this (code, "text/html; charset=utf-8", new MemoryStream(contentStream), closeConnection)
        {
        }

        public static HttpResponse CreateWithMessage(HttpResponseCode code, string message, bool keepAliveConnection, string body = "")
        {
            return new HttpResponse(
                code,
                string.Format(
                    "<html><head><title>{0}</title></head><body><h1>{0}</h1><hr>{1}</body></html>",
                    message, body), keepAliveConnection);
        }
        private static MemoryStream StringToStream(string content)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            return stream;
        }
        public async Task WriteResponse(StreamWriter writer)
        {

            _headerStream.Position = 0;
            await _headerStream.CopyToAsync(writer.BaseStream).ConfigureAwait(false);
            
            ContentStream.Position = 0;
            await ContentStream.CopyToAsync(writer.BaseStream).ConfigureAwait(false);
            await writer.BaseStream.FlushAsync();
        }

        public bool CloseConnection
        {
            get { return _closeConnection; }
        }

        private void WriteHeaders(StreamWriter tempWriter)
        {
            tempWriter.WriteLine("{0} {1} {2}", Protocol, (int)Code, ResponseTexts[(int)Code]);
            tempWriter.WriteLine("Date: {0}", DateTime.UtcNow.ToString("R"));
            tempWriter.WriteLine("Connection: {0}", _closeConnection ? "Close" : "Keep-Alive");
            tempWriter.WriteLine("Content-Type: {0}", ContentType);
            tempWriter.WriteLine("Content-Length: {0}", ContentStream.Length);
            tempWriter.WriteLine();
            tempWriter.Flush();
        }
    }
}