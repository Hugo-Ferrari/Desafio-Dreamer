namespace backend.Exceptions;

public sealed class CredenciaisInvalidasException(string mensagem) : ExcecaoAplicacao(mensagem)
{
    public override int StatusCode => StatusCodes.Status401Unauthorized;

    public override string Titulo => "Não autenticado";
}
