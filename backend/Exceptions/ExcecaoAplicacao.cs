namespace backend.Exceptions;

public abstract class ExcecaoAplicacao(string mensagem) : Exception(mensagem)
{
    public abstract int StatusCode { get; }

    public abstract string Titulo { get; }
}
