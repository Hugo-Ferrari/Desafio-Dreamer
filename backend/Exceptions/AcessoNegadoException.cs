namespace backend.Exceptions;

public sealed class AcessoNegadoException(string mensagem) : ExcecaoAplicacao(mensagem)
{
    public override int StatusCode => StatusCodes.Status403Forbidden;

    public override string Titulo => "Acesso negado";
}
