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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using uhttpsharp.Headers;

namespace uhttpsharp
{
    [DebuggerDisplay("{Method} {OriginalUri,nq}")]
    internal class HttpRequest : IHttpRequest
    {
        private readonly IHttpHeaders _headers;
        private readonly HttpMethods _method;
        private readonly string _protocol;
        private readonly Uri _uri;
        private readonly string[] _requestParameters;
        private readonly IHttpHeaders _queryString;
        private readonly IHttpHeaders _post;

        public HttpRequest(IHttpHeaders headers, HttpMethods method, string protocol, Uri uri, string[] requestParameters, IHttpHeaders queryString, IHttpHeaders post)
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

        internal string OriginalUri
        {
            get
            {
                if (QueryString == null)
                {
                    return Uri.OriginalString;    
                }

                return Uri.OriginalString + "?" + QueryString.ToUriData();

            }
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

            var headersRaw = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            // get the headers
            string line;
            while (!string.IsNullOrEmpty((line = await streamReader.ReadLineAsync().ConfigureAwait(false))))
            { 
                var headerKvp = SplitHeader(line);
                headersRaw.Add(headerKvp.Key, headerKvp.Value);
            }

            IHttpHeaders headers = new HttpHeaders(headersRaw);
            IHttpHeaders post = await GetPostData(streamReader, headers);

            return new HttpRequest(headers, httpMethod, httpProtocol, uri,
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

        private static async Task<IHttpHeaders> GetPostData(StreamReader streamReader, IHttpHeaders headers)
        {
            int postContentLength;
            IHttpHeaders post;
            if (headers.TryGetByName("content-length", out postContentLength))
            {
                post = await HttpHeaders.FromPost(streamReader, postContentLength);
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