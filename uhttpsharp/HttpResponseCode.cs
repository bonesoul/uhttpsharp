// ReSharper disable UnusedMember.Global

namespace uhttpsharp
{
    public enum HttpResponseCode
    {
        Ok = 200,
        Found = 302,
        SeeOther = 303,
        BadRequest = 400,
        NotFound = 404,
        InternalServerError = 500,
        ServerBusy = 502,
    }
}