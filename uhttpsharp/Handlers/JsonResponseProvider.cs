using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace uhttpsharp.Handlers
{
    public class JsonResponseProvider : IResponseProvider
    {
        public Task<IHttpResponse> Provide(object value)
        {
            var memoryStream = new MemoryStream();
            var writer = new JsonTextWriter(new StreamWriter(memoryStream));
            var serializer = new JsonSerializer();
            serializer.Serialize(writer, value);
            writer.Flush();
            return Task.FromResult<IHttpResponse>(new HttpResponse(HttpResponseCode.Ok, "application/json; charset=utf-8", memoryStream, true));
        }
    }
}