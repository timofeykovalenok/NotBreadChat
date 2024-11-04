using System.Net;

namespace Core.Exceptions
{
    public class StatusCodeException : Exception
    {
        public readonly HttpStatusCode StatusCode;

        public StatusCodeException(HttpStatusCode statusCode, string? message = null)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
