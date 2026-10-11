using backend.Models.Enums;

namespace backend.Models;

public class Ausencia
{
    public Guid Id { get; set; }

    public Guid FuncionarioId { get; set; }

    public Funcionario Funcionario { get; set; } = null!;

    public TipoAusencia Tipo { get; set; }

    public DateOnly DataInicio { get; set; }

    public DateOnly DataFim { get; set; }

    public string? Observacoes { get; set; }
}
