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
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private readonly IHttpHeaders _queryString;
        private readonly IHttpHeaders _post;

        public HttpRequestV2(IHttpHeaders headers, HttpMethods method, string protocol, Uri uri, string[] requestParameters, IHttpHeaders queryString, IHttpHeaders post)
        {
            _headers = headers;
            _method = method;
            _protocol = protocol;
            _uri = uri;
            _requestParameters = requestParameters;
            _queryString = queryString;
            _post = post;
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

        public IHttpHeaders Post
        {
            get { return _post; }
        }

        public IHttpHeaders QueryString
        {
            get { return _queryString; }
        }
    }

    public interface IHttpRequest
    {
        IHttpHeaders Headers { get; }

        HttpMethods Method { get; }

        string Protocol { get; }

        Uri Uri { get; }

        string[] RequestParameters { get; }

        IHttpHeaders Post { get; }

        IHttpHeaders QueryString { get; }


    }


    public class EmptyHttpHeaders : IHttpHeaders
    {
        public static readonly IHttpHeaders Empty = new EmptyHttpHeaders();
        
        private static readonly IEnumerable<KeyValuePair<string, string>> EmptyKeyValuePairs = new KeyValuePair<string, string>[0];

        private EmptyHttpHeaders()
        {

        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return EmptyKeyValuePairs.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return EmptyKeyValuePairs.GetEnumerator();
        }
        public string GetByName(string name)
        {
            throw new ArgumentException("EmptyHttpHeaders does not contain any header");
        }
        public bool TryGetByName(string name, out string value)
        {
            value = null;
            return false;
        }
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

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static async Task<IHttpHeaders> FromPost(StreamReader reader, int postContentLength)
        {
            byte[] buffer = new byte[postContentLength];
            var readBytes = await reader.BaseStream.ReadAsync(buffer, 0, postContentLength);

            string body = Encoding.UTF8.GetString(buffer, 0, readBytes);

            return new QueryStringHttpHeaders(body);
        }
    }

    public static class HttpHeadersExtensions
    {
        public static bool KeepAliveConnection(this IHttpHeaders headers)
        {
            string value;
            return headers.TryGetByName("Connection", out value) && value.Equals("Keep-Alive", StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public interface IHttpHeaders : IEnumerable<KeyValuePair<string, string>>
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

            var url = tokens[1];
            var queryString = GetQueryStringData(ref url);
            var uri = new Uri(url, UriKind.Relative);

            var headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            // get the headers
            string line;
            while (!string.IsNullOrEmpty((line = await streamReader.ReadLineAsync().ConfigureAwait(false))))
            { 
                var headerKvp = SplitHeader(line);
                headers.Add(headerKvp.Key, headerKvp.Value);
            }

            IHttpHeaders post = await GetPostData(streamReader, headers);
            
            return new HttpRequestV2(new HttpHeaders(headers), httpMethod, httpProtocol, uri,
                uri.OriginalString.Split(Separators, StringSplitOptions.RemoveEmptyEntries), queryString, post);
        }
        private static IHttpHeaders GetQueryStringData(ref string url)
        {
            var queryStringIndex = url.IndexOf('?');
            IHttpHeaders queryString;
            if (queryStringIndex != -1)
            {
                queryString = new QueryStringHttpHeaders(url.Substring(queryStringIndex + 1));
                url = url.Substring(0, queryStringIndex);
            }
            else
            {
                queryString = EmptyHttpHeaders.Empty;
            }
            return queryString;
        }

        private static async Task<IHttpHeaders> GetPostData(StreamReader streamReader, Dictionary<string, string> headers)
        {
            string postContentLength;
            IHttpHeaders post;
            if (headers.TryGetValue("content-length", out postContentLength))
            {
                post = await HttpHeaders.FromPost(streamReader, int.Parse(postContentLength));
            }
            else
            {
                post = EmptyHttpHeaders.Empty;
            }
            return post;
        }

        private KeyValuePair<string, string> SplitHeader(string header)
        {
            var index = header.IndexOf(": ", StringComparison.InvariantCulture);
            return new KeyValuePair<string, string>(header.Substring(0, index), header.Substring(index + 2).TrimStart(' '));
        }

    }

    public class QueryStringHttpHeaders : IHttpHeaders
    {
        private readonly HttpHeaders _child;
        private static readonly char[] Seperators = {'&', '='};

        public QueryStringHttpHeaders(string query)
        {
            var splittedKeyValues = query.Split(Seperators, StringSplitOptions.RemoveEmptyEntries);
            var values = new Dictionary<string, string>(splittedKeyValues.Length / 2, StringComparer.InvariantCultureIgnoreCase);

            for (int i = 0; i < splittedKeyValues.Length; i += 2)
            {
                var key = Uri.UnescapeDataString(splittedKeyValues[i]);
                var value = Uri.UnescapeDataString(splittedKeyValues[i + 1]).Replace('+', ' ');

                values[key] = value;
            }

            _child = new HttpHeaders(values);
        }

        public string GetByName(string name)
        {
            return _child.GetByName(name);
        }
        public bool TryGetByName(string name, out string value)
        {
            return _child.TryGetByName(name, out value);
        }
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _child.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public sealed class HttpRequestParameters
    {
        private readonly string[] _params;

        private static readonly char[] Separators = { '/' };

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