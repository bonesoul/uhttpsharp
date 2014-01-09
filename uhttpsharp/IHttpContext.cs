using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uhttpsharp
{
    public interface IHttpContext
    {

        IHttpRequest Request { get; }

        IHttpResponse Response { get; set; }

    }


}
