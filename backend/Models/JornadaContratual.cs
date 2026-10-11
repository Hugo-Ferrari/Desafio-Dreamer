namespace backend.Models;

public class JornadaContratual
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public int MinutosDiarios { get; set; }

    public int MinutosSemanais { get; set; }

    public int MinutosIntervaloMinimo { get; set; }

    public ICollection<Funcionario> Funcionarios { get; set; } = [];
}
