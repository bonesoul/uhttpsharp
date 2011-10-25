/*  
 * uhttpsharp - A very lightweight & simple embedded http server for c# - http://code.google.com/p/uhttpsharp/
 * 
 * Copyright (c) 2010, Hüseyin Uslu
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *  
 *   Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * 
 *   Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer 
 *   in the documentation and/or other materials provided with the distribution.
 *   
 *   Neither the name of the uhttpsharp nor the names of its contributors may be used to endorse or promote products derived from 
 *   this software without specific prior written permission.
 *   
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT 
 * NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
 * THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE 
 * OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
