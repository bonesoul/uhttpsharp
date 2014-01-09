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
using System.Data.Odbc;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace uhttpsharp
{
    public class HttpRequestV2 : IHttpRequest
    {
        private readonly IHttpHeaders _headers;
        private readonly HttpMethods _method;
        private readonly string _protocol;
        private readonly Uri _uri;
        private readonly string[] _requestParameters;

        public HttpRequestV2(IHttpHeaders headers, HttpMethods method, string protocol, Uri uri, string[] requestParameters)
        {
            _headers = headers;
            _method = method;
            _protocol = protocol;
            _uri = uri;
            _requestParameters = requestParameters;
        }

        public IHttpHeaders Headers
        {
            get { return _headers; }
        }

        public HttpMethods Method
        {
            get { return _method; }
        }

        public string Protocol
        {
            get { return _protocol; }
        }

        public Uri Uri
        {
            get { return _uri; }
        }

        public string[] RequestParameters
        {
            get { return _requestParameters; }
        }
    }

    public interface IHttpRequest
    {
        IHttpHeaders Headers { get; }

        HttpMethods Method { get; }

        string Protocol { get; }

        Uri Uri { get; }

        string[] RequestParameters { get; }


    }

    public class HttpHeaders : IHttpHeaders
    {
        private readonly IDictionary<string, string> _values;
        public HttpHeaders(IDictionary<string, string> values)
        {
            _values = values;
        }

        public string GetByName(string name)
        {
            return _values[name];
        }
        public bool TryGetByName(string name, out string value)
        {
            return _values.TryGetValue(name, out value);
        }
    }

    public interface IHttpHeaders
    {

        string GetByName(string name);

        bool TryGetByName(string name, out string value);

    }

    public interface IHttpRequestProvider
    {
        /// <summary>
        /// Provides an <see cref="IHttpRequest"/> based on the context of the stream,
        /// May return null / throw exceptions on invalid requests.
        /// </summary>
        /// <param name="streamReader"></param>
        /// <returns></returns>
        Task<IHttpRequest> Provide(StreamReader streamReader);

    }

    public class HttpRequestProvider : IHttpRequestProvider
    {
        private static readonly char[] Separators = { '/' };

        public async Task<IHttpRequest> Provide(StreamReader streamReader)
        {
            // parse the http request
            var request = await streamReader.ReadLineAsync().ConfigureAwait(false);
            
            if (request == null)
                return null;

            var tokens = request.Split(' ');

            if (tokens.Length != 3)
            {
                return null;
            }

            var httpMethod = HttpMethodProvider.Default.Provide(tokens[0]);
            var httpProtocol = tokens[2];
            var uri = new Uri(tokens[1], UriKind.Relative);
             
            var headers = new Dictionary<string, string>();

            // get the headers
            string line;
            while ((line = await streamReader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                if (line.Equals(string.Empty)) break;
                var headerKvp = SplitHeader(line);
                headers.Add(headerKvp.Key, headerKvp.Value);
            }

            return new HttpRequestV2(new HttpHeaders(headers), httpMethod, httpProtocol, uri, uri.OriginalString.Split(Separators, StringSplitOptions.RemoveEmptyEntries));
        }

        private KeyValuePair<string, string> SplitHeader(string header)
        {
            var index = header.IndexOf(": ", StringComparison.InvariantCulture);
            return new KeyValuePair<string, string>(header.Substring(0, index), header.Substring(index + 2).TrimStart(' '));
        }
        
    }


    public sealed class HttpRequestParameters
    {
        private readonly string[] _params;

        private static readonly char[] Separators = {'/'};

        public HttpRequestParameters(Uri uri)
        {
            var url = uri.OriginalString;
            _params = url.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
        }

        public IList<string> Params
        {
            get { return _params; }
        }
    }
}