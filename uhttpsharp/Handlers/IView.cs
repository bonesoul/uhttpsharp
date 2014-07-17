using Newtonsoft.Json;

namespace uhttpsharp.Handlers
{
    public interface IView
    {
        string Stringify(object state);
    }

    public class JsonView : IView
    {
        public string Stringify(object state)
        {
            return JsonConvert.SerializeObject(state);
        }
    }
}