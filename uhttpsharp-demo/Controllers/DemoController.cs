using System;
using System.Linq;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Attributes;
using uhttpsharp.Controllers;
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
    public class MyRequest : IValidate
    {
        public int A { get; set; }
        public void Validate(IErrorContainer container)
        {
            if (A == 0)
            {
                container.Log("A cannot be zero");
            }
        }
    }
    class BaseController : IController
    {
        [HttpMethod(HttpMethods.Get)]
        public Task<IControllerResponse> Get()
        {
            return Response.Render(HttpResponseCode.Ok, new {Hello="Base!", Kaki = Enumerable.Range(0, 10000)});
        }

        [HttpMethod(HttpMethods.Post)]
        public Task<IControllerResponse> Post([FromBody] MyRequest a)
        {
            return Response.Render(HttpResponseCode.Ok, a);
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
        [HttpMethod(HttpMethods.Get)]
        public new Task<IControllerResponse> Get()
        {
            return Response.Render(HttpResponseCode.Ok, new { Hello = "Derived!" });
        }
    }
}
