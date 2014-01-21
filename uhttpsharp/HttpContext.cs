using System.Dynamic;
using uhttpsharp.Headers;

namespace uhttpsharp
{
    internal class HttpContext : IHttpContext
    {
        private readonly IHttpRequest _request;
        private readonly ICookiesStorage _cookies;
        private readonly ExpandoObject _state = new ExpandoObject();
        public HttpContext(IHttpRequest request)
        {
            _request = request;
            _cookies = new CookiesStorage(_request.Headers.GetByNameOrDefault("cookie", string.Empty));
        }

        public IHttpRequest Request
        {
            get { return _request; }
        }

        public IHttpResponse Response { get; set; }

        public ICookiesStorage Cookies
        {
            get { return _cookies; }
        }


        public dynamic State
        {
            get { return _state; }
        }
    }
}