using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace uhttpsharp.Handlers
{
    public interface IRestController<T>
    {
        /// <summary>
        /// Returns a list of object that found in the collection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> Get(IHttpRequest request);

        /// <summary>
        /// Returns an item from the collection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<T> GetItem(IHttpRequest request);

        /// <summary>
        /// Creates a new entry in the collection - new uri is returned
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<T> Create(IHttpRequest request);

        /// <summary>
        /// Updates an entry in the collection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<T> Upsert(IHttpRequest request);

        /// <summary>
        /// Removes an entry from the collection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<T> Delete(IHttpRequest request);
    }

    public class JsonRestControllerAdapter<T> : IRestController
    {
        private readonly IRestController<T> _controller;

        public JsonRestControllerAdapter(IRestController<T> controller)
        {
            _controller = controller;
        }

        private Task<HttpResponse> CreateHttpRequest(T value)
        {
            if (EqualityComparer<T>.Default.Equals(value,default(T)))
            {
                return GenerateNotFonudResponse();
            }

            return SerializeToHttpResponse(value);
        }
        private static Task<HttpResponse> GenerateNotFonudResponse()
        {
            return Task.FromResult(new HttpResponse(HttpResponseCode.NotFound, string.Empty));
        }
        private Task<HttpResponse> CreateHttpRequest(IEnumerable<T> value)
        {
            if (value == null)
            {
                return GenerateNotFonudResponse();
            }

            var values = value.ToList();

            if (values.Count == 0)
            {
                return GenerateNotFonudResponse();
            }

            return SerializeToHttpResponse(values);
        }
        private static Task<HttpResponse> SerializeToHttpResponse<TObj>(TObj value)
        {
            var memoryStream = new MemoryStream();
            var writer = new JsonTextWriter(new StreamWriter(memoryStream));
            var serializer = new JsonSerializer();
            serializer.Serialize(writer, value);
            writer.Flush();
            return Task.FromResult(new HttpResponse(HttpResponseCode.Ok, "application/json; charset=utf-8", memoryStream));
        }

        public async Task<HttpResponse> Get(IHttpRequest request)
        {
            return await CreateHttpRequest(await _controller.Get(request).ConfigureAwait(false)).ConfigureAwait(false);
        }
        public async Task<HttpResponse> GetItem(IHttpRequest request)
        {
            return await CreateHttpRequest(await _controller.GetItem(request).ConfigureAwait(false)).ConfigureAwait(false);
        }
        public async Task<HttpResponse> Create(IHttpRequest request)
        {
            return await CreateHttpRequest(await _controller.Create(request).ConfigureAwait(false)).ConfigureAwait(false);
        }
        public async Task<HttpResponse> Upsert(IHttpRequest request)
        {
            return await CreateHttpRequest(await _controller.Upsert(request).ConfigureAwait(false)).ConfigureAwait(false);
        }
        public async Task<HttpResponse> Delete(IHttpRequest request)
        {
            return await CreateHttpRequest(await _controller.Delete(request).ConfigureAwait(false)).ConfigureAwait(false);
        }
    }
}
