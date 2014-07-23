using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Attributes;
using uhttpsharp.Handlers;

namespace uhttpsharpdemo.Controllers
{
    public class EmptyPipeline : IPipeline
    {
        public Task<IControllerResponse> Go(Func<Task<IControllerResponse>> injectedTask, IHttpContext context)
        {
            return injectedTask();
        }
    }

    public class JsonController : IController
    {
        public class Question
        {
            public string TheQuestion { get; set; }
        }
        public JsonController(int id)
        {
        }

        [HttpMethod(HttpMethods.Post)]
        public Task<IControllerResponse> Post([FromBody] Question question)
        {
            return Response.Render(HttpResponseCode.Ok, question);
        }
        public IPipeline Pipeline
        {
            get { return new EmptyPipeline(); }
        }
    }
    public class MyController
    {
        private readonly int _id;
        public MyController(int id)
        {
            _id = id;
        }
        public MyController()
        {
        }

        [HttpMethod(HttpMethods.Post)]
        public Task<IControllerResponse> Post([FromPost("a")] MyRequest request, [FromHeaders("header")]string hello, [FromQuery("query")]string world)
        {
            return Response.Render(HttpResponseCode.Ok, null);
        }

        [Indexer]
        public async Task<object> Get(IHttpContext context, int id)
        {
            return new MyController(id);
        }
    }
    public class MyRequest
    {
        public int A { get; set; }
    }
    class BaseController : IController
    {
        [HttpMethod(HttpMethods.Get)]
        protected Task<IControllerResponse> Get()
        {
            return Response.Render(HttpResponseCode.Ok, new {Hello="Base!", Kaki = Enumerable.Range(0, 10000)});
        }
        public virtual IPipeline Pipeline
        {
            get { return new EmptyPipeline(); }
        }

        public IController Derived {
            get { return new DerivedController(); }
        }
    }

    class DerivedController : BaseController
    {
        protected new Task<IControllerResponse> Get()
        {
            return Response.Render(HttpResponseCode.Ok, new { Hello = "Derived!" });
        }
    }
}
