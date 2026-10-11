namespace backend.Exceptions;

public sealed class ConflitoException(string mensagem) : ExcecaoAplicacao(mensagem)
{
    public override int StatusCode => StatusCodes.Status409Conflict;

    public override string Titulo => "Conflito";
}
