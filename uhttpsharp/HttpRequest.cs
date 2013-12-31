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
    public sealed class HttpRequest
    {
        public static async Task<HttpRequest> Build(Stream stream)
        {
            var retVal = new HttpRequest(stream);
            await retVal.Process();

            return retVal;
        }

        public bool Valid { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public HttpMethod HttpMethod { get; private set; }
        public string HttpProtocol { get; private set; }
        public Uri Uri { get; private set; }
        public string URL { get; private set; }
        public HttpRequestParameters Parameters { get; private set; }

        private readonly Stream _stream;
        private readonly StreamReader _streamReader;

        public HttpRequest(Stream stream)
        {
            Headers = new Dictionary<string, string>();
            _stream = stream;
            _streamReader = new StreamReader(_stream);
        }

        private async Task Process()
        {
            Valid = false;

            // parse the http request
            var request = await _streamReader.ReadLineAsync();
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
            Uri = new Uri(URL, UriKind.Relative);
             
            Parameters = new HttpRequestParameters(URL);

            Console.WriteLine("[{0}:{1}] URL: {2}", HttpProtocol, HttpMethod, URL);

            // get the headers
            string line;
            while ((line = await _streamReader.ReadLineAsync()) != null)
            {
                if (line.Equals(string.Empty)) break;
                var keys = line.Split(':');
                Headers.Add(keys[0], keys[1]);
            }

            Valid = true;
        }

        private KeyValuePair<string, string> SplitHeader(string header)
        {
            var index = header.IndexOf(':');
            return new KeyValuePair<string, string>(header.Substring(0, index), header.Substring(index + 1));
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