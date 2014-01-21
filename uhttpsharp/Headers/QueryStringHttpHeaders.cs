using System;
using System.Collections;
using System.Collections.Generic;

namespace uhttpsharp.Headers
{
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
}