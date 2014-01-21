using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using uhttpsharp.Headers;

namespace uhttpsharp.RequestProviders
{
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
            var index = header.IndexOf(": ", StringComparison.InvariantCultureIgnoreCase);
            return new KeyValuePair<string, string>(header.Substring(0, index), header.Substring(index + 2));
        }

    }
}