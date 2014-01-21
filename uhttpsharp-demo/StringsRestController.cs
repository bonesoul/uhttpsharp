using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using uhttpsharp;
using uhttpsharp.Handlers;

namespace uhttpsharpdemo
{
    class StringsRestController : IRestController<string>
    {
        private readonly ICollection<string> _collection = new HashSet<string>();  

        public Task<IEnumerable<string>> Get(IHttpRequest request)
        {
            return Task.FromResult<IEnumerable<string>>(_collection);
        }
        public Task<string> GetItem(IHttpRequest request)
        {
            var id = GetId(request);

            if (_collection.Contains(id))
            {
                return Task.FromResult(id);
            }

            throw GetNotFoundException();
        }
        private static string GetId(IHttpRequest request)
        {
            var id = request.RequestParameters[1];

            return id;
        }
        public Task<string> Create(IHttpRequest request)
        {
            var id = GetId(request);

            _collection.Add(id);

            return Task.FromResult(id);
        }
        public Task<string> Upsert(IHttpRequest request)
        {
            return Create(request);
        }
        public Task<string> Delete(IHttpRequest request)
        {
            var id = GetId(request);

            if (_collection.Remove(id))
            {
                return Task.FromResult(id);
            }

            throw GetNotFoundException();
        }
        private static Exception GetNotFoundException()
        {
            return new HttpException(HttpResponseCode.NotFound, "The resource you've looked for is not found");
        }
    }
}