namespace backend.Exceptions;

public sealed class RecursoNaoEncontradoException(string mensagem) : ExcecaoAplicacao(mensagem)
{
    public override int StatusCode => StatusCodes.Status404NotFound;

    public override string Titulo => "Recurso não encontrado";
}
