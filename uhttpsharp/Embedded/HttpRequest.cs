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
using System.Threading;

namespace uhttpsharp.Embedded
{
    public sealed class HttpRequest
    {
        public bool Valid { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public HttpMethod HttpMethod { get; private set; }
        public string HttpProtocol { get; private set; }
        public Uri Uri { get; private set; }
        public string URL { get; private set; }
        public HttpRequestParameters Parameters { get; private set; }

        private readonly Stream _stream;

        public HttpRequest(Stream stream)
        {
            Headers = new Dictionary<string, string>();
            _stream = stream;
            Process();
        }

        private void Process()
        {
            Valid = false;

            // parse the http request
            var request = ReadLine();
            if (request == null)
                return;
            var tokens = request.Split(' ');

            if (tokens.Length != 3)
            {
                Console.WriteLine("httpserver: invalid http request.");
                return;
            }

            switch (tokens[0].ToUpper())
            {
                case "GET":
                    HttpMethod = HttpMethod.Get;
                    break;
                case "POST":
                    HttpMethod = HttpMethod.Post;
                    break;
            }

            HttpProtocol = tokens[2];
            URL = tokens[1];
            Uri = new Uri("http://" + HttpServer.Instance.Address + "/" + URL.TrimStart('/'));
            Parameters = new HttpRequestParameters(URL);

            Console.WriteLine(string.Format("[{0}:{1}] URL: {2}", HttpProtocol, HttpMethod, URL));

            // get the headers
            string line;
            while ((line = ReadLine()) != null)
            {
                if (line.Equals("")) break;
                var keys = line.Split(':');
                Headers.Add(keys[0], keys[1]);
            }

            Valid = true;
        }

        private string ReadLine()
        {
            var buffer = string.Empty;

            while (true)
            {
                var _char = _stream.ReadByte();
                if (_char == '\n') break;
                if (_char == '\r') continue;
                if (_char == -1)
                {
                    if (buffer != string.Empty)
                        return buffer;
                    return null;
                }
                buffer += Convert.ToChar(_char);
            }

            return buffer;
        }
    }

    public enum HttpMethod
    {
        Get,
        Post
    }

    public sealed class HttpRequestParameters
    {
        public string Function { get; private set; }
        public string[] Params { get; private set; }

        public HttpRequestParameters(string url)
        {
            Function = "";
            var tokens = url.Split('/');

            if (tokens.Length > 1)
            {
                Function = tokens[1];
                Params = new string[tokens.Length - 2];
                for (var i = 2; i < tokens.Length; i++)
                {
                    Params[i - 2] = tokens[i];
                }
            }
        }
    }
}