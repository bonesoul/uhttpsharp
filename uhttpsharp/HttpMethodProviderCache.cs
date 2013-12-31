using System.Collections.Concurrent;

namespace uhttpsharp
{
    public class HttpMethodProviderCache : IHttpMethodProvider
    {
        private readonly ConcurrentDictionary<string, HttpMethods> _cache = new ConcurrentDictionary<string, HttpMethods>(); 

        private readonly IHttpMethodProvider _child;
        public HttpMethodProviderCache(IHttpMethodProvider child)
        {
            _child = child;
        }
        public HttpMethods Provide(string name)
        {
            return _cache.GetOrAdd(name, _child.Provide);
        }
    }
}