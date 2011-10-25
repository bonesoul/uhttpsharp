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
        public string URL { get; private set; }
        public HttpRequestParameters Parameters { get; private set; }

        private Stream _stream;

        public HttpRequest(Stream stream)
        {
            this.Headers = new Dictionary<string, string>();
            this._stream = stream;
            this.Process();
        }

        private void Process()
        {
            this.Valid = false;

            // parse the http request
            string request = this.ReadLine();
            string[] tokens = request.Split(' ');

            if (tokens.Length != 3)
            {
                Console.WriteLine("httpserver: invalid http request.");
                return;
            }

            switch (tokens[0].ToUpper())
            {
                case "GET": this.HttpMethod = HttpMethod.GET; break;
                case "POST": this.HttpMethod = HttpMethod.POST; break;
            }
                        
            this.HttpProtocol = tokens[2];
            this.URL = tokens[1];
            this.Parameters = new HttpRequestParameters(this.URL);

            Console.WriteLine(string.Format("[{0}:{1}] URL: {2}", this.HttpProtocol, this.HttpMethod, this.URL));

            // get the headers
            string line;
            while ((line = this.ReadLine()) != null)
            {
                if (line.Equals("")) break;
                string[] keys = line.Split(':');
                this.Headers.Add(keys[0], keys[1]);
            }

            this.Valid = true;
        }

        private string ReadLine()
        {
            string buffer = string.Empty;
            int _char;

            while (true)
            {
                _char = this._stream.ReadByte();
                if (_char == '\n') break;
                if (_char == '\r') continue;
                if (_char == -1) { Thread.Sleep(10); continue; }
                buffer += Convert.ToChar(_char);
            }

            return buffer;
        }
    }

    public enum HttpMethod
    {
        GET,
        POST
    }

    public sealed class HttpRequestParameters
    {
        public string Function { get; private set; }
        public string[] Params { get; private set; }

        public HttpRequestParameters(string url)
        {
            this.Function = "";
            string[] tokens = url.Split('/');

            if (tokens.Length > 1)
            {
                this.Function = tokens[1];
                this.Params = new string[tokens.Length - 2];
                for (int i = 2; i < tokens.Length; i++)
                {
                    this.Params[i - 2] = tokens[i];
                }
            }
        }

    }
}
