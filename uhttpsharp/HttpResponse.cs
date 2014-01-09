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
using System.Threading.Tasks;

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
        public bool CloseConnection { get; private set; }
        public HttpResponseCode Code { get; private set; }
        private Stream ContentStream { get; set; }

        public HttpResponse(HttpResponseCode code, string content)
            : this(code, "text/html; charset=utf-8", StringToStream(content))
        {
        }
        public HttpResponse(string contentType, Stream contentStream)
            : this(HttpResponseCode.Ok, contentType, contentStream)
        {
        }
        private HttpResponse(HttpResponseCode code, string contentType, Stream contentStream)
        {
            Protocol = "HTTP/1.1";
            ContentType = contentType;
            CloseConnection = true;

            Code = code;
            ContentStream = contentStream;
        }
        public HttpResponse(HttpResponseCode code, byte[] contentStream) 
            : this (code, "text/html; charset=utf-8", new MemoryStream(contentStream))
        {
        }

        public static HttpResponse CreateWithMessage(HttpResponseCode code, string message, string body = "")
        {
            return new HttpResponse(
                code,
                string.Format(
                    "<html><head><title>{0}</title></head><body><h1>{0}</h1><hr>{1}</body></html>",
                    message, body));
        }
        private static Stream StringToStream(string content)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            return stream;
        }
        public async Task WriteResponse(StreamWriter writer)
        {
            using (var memoryStream = new MemoryStream())
            using (var tempWriter = new StreamWriter(memoryStream))
            {
                WriteHeaders(tempWriter);
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(writer.BaseStream).ConfigureAwait(false);
            }

            ContentStream.Position = 0;
            await ContentStream.CopyToAsync(writer.BaseStream).ConfigureAwait(false);
            await writer.BaseStream.FlushAsync();
        }

        private void WriteHeaders(StreamWriter tempWriter)
        {
            tempWriter.WriteLine("{0} {1} {2}", Protocol, (int)Code, ResponseTexts[(int)Code]);
            tempWriter.WriteLine("Date: {0}", DateTime.UtcNow.ToString("R"));
            tempWriter.WriteLine("Connection: {0}", CloseConnection ? "close" : "Keep-Alive");
            tempWriter.WriteLine("Content-Type: {0}", ContentType);
            tempWriter.WriteLine("Content-Length: {0}", ContentStream.Length);
            tempWriter.WriteLine();
            tempWriter.Flush();
        }
    }
}