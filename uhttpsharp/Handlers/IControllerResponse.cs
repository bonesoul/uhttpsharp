using System;
using System.Threading.Tasks;

namespace uhttpsharp.Handlers
{
    public interface IControllerResponse
    {
        Task<IHttpResponse> Respond(IHttpContext context, IView view);
    }

    public class RenderResponse : IControllerResponse
    {
        private readonly HttpResponseCode _code;
        private readonly object _state;
        public RenderResponse(HttpResponseCode code, object state)
        {
            _code = code;
            _state = state;
        }
        public object State
        {
            get { return _state; }
        }
        public HttpResponseCode Code
        {
            get { return _code; }
        }
        public Task<IHttpResponse> Respond(IHttpContext context, IView view)
        {
            var output = view.Stringify(_state);
            return Task.FromResult<IHttpResponse>(new HttpResponse(_code, output, true));
        }
    }

    public class RedirectResponse : IControllerResponse
    {
        public Task<IHttpResponse> Respond(IHttpContext context, IView view)
        {
            return Task.FromResult<IHttpResponse>(new HttpResponse(HttpResponseCode.Found, String.Empty, false));
        }
    }

    public static class Response
    {
        public static Task<IControllerResponse> Create(IControllerResponse response)
        {
            return Task.FromResult(response);
        }
        public static Task<IControllerResponse> Render(HttpResponseCode code, object state)
        {
            return Create(new RenderResponse(code, state));
        }
        public static Task<IControllerResponse> Redirect()
        {
            return Create(new RedirectResponse());
        }
    }
}