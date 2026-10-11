namespace backend.Models;

public class Pais
{
    public string Codigo { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string FusoHorarioPadrao { get; set; } = string.Empty;

    public ICollection<Funcionario> Funcionarios { get; set; } = [];

    public ICollection<Feriado> Feriados { get; set; } = [];

    public ICollection<FechamentoPeriodo> Fechamentos { get; set; } = [];
}
