using System;
using System.Threading.Tasks;

namespace uhttpsharp.Handlers
{
    public interface IPipeline
    {
        Task<IControllerResponse> Go(Func<Task<IControllerResponse>> injectedTask, IHttpContext context);
    }
}