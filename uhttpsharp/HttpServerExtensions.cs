using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uhttpsharp
{
    public static class HttpServerExtensions
    {

        public static void  Use(this HttpServer server, Func<IHttpRequest, Func<Task<IHttpResponse>>, Task<IHttpResponse>> method)
        {
            server.Use(new AnonymousHttpRequestHandler(method));
        }

    }

    public class AnonymousHttpRequestHandler : IHttpRequestHandler
    {
        private readonly Func<IHttpRequest, Func<Task<IHttpResponse>>, Task<IHttpResponse>> _method;

        public AnonymousHttpRequestHandler(Func<IHttpRequest, Func<Task<IHttpResponse>>, Task<IHttpResponse>> method)
        {
            _method = method;
        }

        public Task<IHttpResponse> Handle(IHttpRequest httpRequest, Func<Task<IHttpResponse>> next)
        {
            return _method(httpRequest, next);
        }
    }
}
