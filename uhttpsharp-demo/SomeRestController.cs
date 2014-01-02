using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using uhttpsharp;
using uhttpsharp.Handlers;

namespace uhttpsharpdemo
{

    class SomeRestControllerOfT : IRestController<string>
    {
        public Task<IEnumerable<string>> Get(IHttpRequest request)
        {
            return Task.FromResult(new[] {"1", "2"}.AsEnumerable());
        }
        public Task<string> GetItem(IHttpRequest request)
        {
            throw new NotImplementedException();
        }
        public Task<string> Create(IHttpRequest request)
        {
            throw new NotImplementedException();
        }
        public Task<string> Upsert(IHttpRequest request)
        {
            throw new NotImplementedException();
        }
        public Task<string> Delete(IHttpRequest request)
        {
            throw new NotImplementedException();
        }
    }

    class SomeRestController : IRestController
    {

        IDictionary<int, string> _strings = new Dictionary<int, string>() { { 1 , "Hahaha"}};

        public Task<uhttpsharp.HttpResponse> Get(uhttpsharp.IHttpRequest request)
        {
            var memoryStream = new MemoryStream();
            Newtonsoft.Json.JsonWriter writer = new JsonTextWriter(new StreamWriter( memoryStream));

            JsonSerializer.Create().Serialize(writer, _strings);
            writer.Flush();
            return Task.FromResult(new HttpResponse(HttpResponseCode.Ok, "application/json; charset=utf-8", memoryStream));
        }

        public Task<uhttpsharp.HttpResponse> GetItem(uhttpsharp.IHttpRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<uhttpsharp.HttpResponse> Create(uhttpsharp.IHttpRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<uhttpsharp.HttpResponse> Upsert(uhttpsharp.IHttpRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<uhttpsharp.HttpResponse> Delete(uhttpsharp.IHttpRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
