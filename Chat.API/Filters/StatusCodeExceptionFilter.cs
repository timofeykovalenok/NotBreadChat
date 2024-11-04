using Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Core.Filters
{
    public class StatusCodeExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception as StatusCodeException;
            if (exception == null)
                return;

            context.Result = new StatusCodeResult((int)exception.StatusCode);
            context.ExceptionHandled = true;
        }
    }
}
