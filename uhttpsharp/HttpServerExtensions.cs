using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uhttpsharp
{
    public static class HttpServerExtensions
    {

        public static void  Use(this HttpServer server, Func<IHttpRequest, Func<Task<HttpResponse>>, Task<HttpResponse>> method)
        {
            server.Use(new AnonymousHttpRequestHandler(method));
        }

    }

    public class AnonymousHttpRequestHandler : IHttpRequestHandler
    {
        private readonly Func<IHttpRequest, Func<Task<HttpResponse>>, Task<HttpResponse>> _method;

        public AnonymousHttpRequestHandler(Func<IHttpRequest, Func<Task<HttpResponse>>, Task<HttpResponse>> method)
        {
            _method = method;
        }

        public Task<HttpResponse> Handle(IHttpRequest httpRequest, Func<Task<HttpResponse>> next)
        {
            return _method(httpRequest, next);
        }
    }
}
