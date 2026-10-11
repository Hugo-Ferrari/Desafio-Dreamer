namespace backend.Exceptions;

public sealed class RegraNegocioException(string mensagem) : ExcecaoAplicacao(mensagem)
{
    public override int StatusCode => StatusCodes.Status422UnprocessableEntity;

    public override string Titulo => "Regra de negócio violada";
}
