using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using uhttpsharp;

namespace uhttpsharpdemo
{
    class SomeRestController
    {

        IDictionary<int, string> _strings = new Dictionary<int, string>() { { 1 , "Hahaha"}};

        public Task<uhttpsharp.HttpResponse> Get(uhttpsharp.IHttpRequest request)
        {
            var memoryStream = new MemoryStream();
            Newtonsoft.Json.JsonWriter writer = new JsonTextWriter(new StreamWriter( memoryStream));

            JsonSerializer.Create().Serialize(writer, _strings);
            writer.Flush();
            return Task.FromResult(new HttpResponse(HttpResponseCode.Ok, "application/json; charset=utf-8", memoryStream, true));
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
