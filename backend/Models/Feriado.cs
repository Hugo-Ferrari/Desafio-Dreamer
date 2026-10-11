namespace backend.Models;

public class Feriado
{
    public Guid Id { get; set; }

    public string CodigoPais { get; set; } = string.Empty;

    public Pais Pais { get; set; } = null!;

    public DateOnly Data { get; set; }

    public string Nome { get; set; } = string.Empty;
}
