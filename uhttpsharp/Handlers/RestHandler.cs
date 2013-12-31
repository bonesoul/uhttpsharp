using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace uhttpsharp.Handlers
{
    public class RestHandler : IHttpRequestHandler
    {

        private struct RestCall
        {
            private readonly HttpMethods _method;
            private readonly bool _entryFull;

            public RestCall(HttpMethods method, bool entryFull)
            {
                _method = method;
                _entryFull = entryFull;
            }

            public static RestCall Create(HttpMethods method, bool entryFull)
            {
                return new RestCall(method, entryFull);
            }

            private bool Equals(RestCall other)
            {
                return _method == other._method && _entryFull.Equals(other._entryFull);
            }
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is RestCall && Equals((RestCall)obj);
            }
            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int)_method*397) ^ _entryFull.GetHashCode();
                }
            }
        }

        private static readonly IDictionary<RestCall, Func<IRestController, IHttpRequest, Task<HttpResponse>>> RestCallHandlers = new Dictionary<RestCall, Func<IRestController, IHttpRequest, Task<HttpResponse>>>();

        static RestHandler()
        {
            RestCallHandlers.Add(RestCall.Create(HttpMethods.Get, false), (c, r) => c.Get(r));
            RestCallHandlers.Add(RestCall.Create(HttpMethods.Get, true), (c, r) => c.GetItem(r));
            RestCallHandlers.Add(RestCall.Create(HttpMethods.Post, false), (c, r) => c.Create(r));
            RestCallHandlers.Add(RestCall.Create(HttpMethods.Put, true), (c, r) => c.Upsert(r));
            RestCallHandlers.Add(RestCall.Create(HttpMethods.Delete, true), (c, r) => c.Delete(r));
        }

        private readonly IRestController _controller;
        public RestHandler(IRestController controller)
        {
            _controller = controller;
        }

        public async Task<HttpResponse> Handle(IHttpRequest httpRequest, Func<Task<HttpResponse>> next)
        {
            var call = new RestCall(httpRequest.Method, httpRequest.RequestParameters.Length > 1);

            Func<IRestController, IHttpRequest, Task<HttpResponse>> handler;
            if (RestCallHandlers.TryGetValue(call, out handler))
            {
                var value = await handler(_controller, httpRequest);
                return value;
            }

            return await next();
        }
    }



}
