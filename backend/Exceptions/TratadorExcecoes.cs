using Microsoft.AspNetCore.Diagnostics;

namespace backend.Exceptions;

public sealed class TratadorExcecoes(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        if (exception is not ExcecaoAplicacao excecao)
        {
            return false;
        }

        httpContext.Response.StatusCode = excecao.StatusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = excecao,
            ProblemDetails =
            {
                Status = excecao.StatusCode,
                Title = excecao.Titulo,
                Detail = excecao.Message
            }
        });
    }
}
