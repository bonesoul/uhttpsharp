using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using uhttpsharp.Headers;

namespace uhttpsharp.ModelBinders
{
    public class JsonModelBinder : IModelBinder
    {
        public JsonModelBinder()
        {   
        }
        public T Get<T>(byte[] raw)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(raw));
        }
        public T Get<T>(IHttpHeaders headers)
        {
            throw new NotSupportedException();
        }
        public T Get<T>(IHttpHeaders headers, string prefix)
        {
            throw new NotSupportedException();
        }
    }
}
