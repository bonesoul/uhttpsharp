using System.Collections.Generic;
using System.Threading.Tasks;

namespace uhttpsharp.Handlers
{
    public interface IRestController
    {

        /// <summary>
        /// Returns a list of object that found in the collection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<HttpResponse> Get(IHttpRequest request);

        /// <summary>
        /// Returns an item from the collection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<HttpResponse> GetItem(IHttpRequest request);

        /// <summary>
        /// Creates a new entry in the collection - new uri is returned
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<HttpResponse> Create(IHttpRequest request);

        /// <summary>
        /// Updates an entry in the collection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<HttpResponse> Upsert(IHttpRequest request);

        /// <summary>
        /// Removes an entry from the collection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<HttpResponse> Delete(IHttpRequest request);

    }
}