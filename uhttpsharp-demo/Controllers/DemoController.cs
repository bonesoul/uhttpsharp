using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Handlers;

namespace uhttpsharpdemo.Controllers
{
    public class JsonController
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
    class DemoController
    {
    }
}
