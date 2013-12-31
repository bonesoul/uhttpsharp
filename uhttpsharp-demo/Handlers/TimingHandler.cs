using System;
using System.Diagnostics;
using System.Reflection;
using log4net;
using uhttpsharp;

namespace uhttpsharpdemo.Handlers
{
    public class TimingHandler : IHttpRequestHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public async System.Threading.Tasks.Task<HttpResponse> Handle(IHttpRequest httpRequest, Func<System.Threading.Tasks.Task<HttpResponse>> next)
        {
            var stopWatch = Stopwatch.StartNew();
            var retVal = await next();
            
            Logger.InfoFormat("request {0} took {1}", httpRequest.Uri, stopWatch.Elapsed);

            return retVal;
        }
    }
}