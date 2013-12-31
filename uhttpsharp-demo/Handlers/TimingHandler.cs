using System;
using System.Diagnostics;
using uhttpsharp;

namespace uhttpsharpdemo.Handlers
{
    public class TimingHandler : IHttpRequestHandler
    {

        public async System.Threading.Tasks.Task<HttpResponse> Handle(IHttpRequest httpRequest, Func<System.Threading.Tasks.Task<HttpResponse>> next)
        {
            var stopWatch = Stopwatch.StartNew();
            var retVal = await next();
            Console.WriteLine(httpRequest.Uri + " : " + stopWatch.Elapsed);
            return retVal;
        }
    }
}