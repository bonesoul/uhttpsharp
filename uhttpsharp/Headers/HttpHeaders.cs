using System;
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
    internal class ListHttpHeaders : IHttpHeaders
    {
        private readonly IList<KeyValuePair<string, string>> _values;
        public ListHttpHeaders(IList<KeyValuePair<string, string>> values)
        {
            _values = values;
        }

        public string GetByName(string name)
        {
            return _values.Where(kvp => kvp.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Select(kvp => kvp.Value).First();
        }

        public bool TryGetByName(string name, out string value)
        {
            value = _values.Where(kvp => kvp.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Select(kvp => kvp.Value).FirstOrDefault();

            return (value != default(string));
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal int Count
        {
            get
            {
                return _values.Count;
            }
        }
    }


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
}