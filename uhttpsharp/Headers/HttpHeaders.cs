using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uhttpsharp.Headers
{
    [DebuggerDisplay("{Count} Headers")]
    [DebuggerTypeProxy(typeof(HttpHeadersDebuggerProxy))]
    internal class HttpHeaders : IHttpHeaders
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

        internal int Count
        {
            get
            {
                return _values.Count;
            }
        }
    }

    internal class HttpHeadersDebuggerProxy
    {
        private readonly IHttpHeaders _real;
        
        [DebuggerDisplay("{Value,nq}", Name = "{Key,nq}")]
        internal class HttpHeader
        {
            private readonly KeyValuePair<string, string> _header;
            public HttpHeader(KeyValuePair<string, string> header)
            {
                _header = header;
            }

            public string Value
            {
                get
                {
                    return _header.Value;
                }
            }

            public string Key
            {
                get
                {
                    return _header.Key;
                }
            }
        }

        public HttpHeadersDebuggerProxy(IHttpHeaders real)
        {
            _real = real;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public HttpHeader[] Headers
        {
            get
            {
                return _real.Select(kvp => new HttpHeader(kvp)).ToArray();
            }
        }

    }
}