using backend.Models.Enums;

namespace backend.Models;

public class Funcionario
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string HashSenha { get; set; } = string.Empty;

    public PerfilFuncionario Perfil { get; set; }

    public string CodigoPais { get; set; } = string.Empty;

    public Pais Pais { get; set; } = null!;

    public string FusoHorarioPadrao { get; set; } = string.Empty;

    public Guid JornadaContratualId { get; set; }

    public JornadaContratual JornadaContratual { get; set; } = null!;

    public Guid? GestorId { get; set; }

    public Funcionario? Gestor { get; set; }

    public bool Ativo { get; set; } = true;

    public bool DeveAlterarSenha { get; set; }

    public DateTime CriadoEmUtc { get; set; }

    public ICollection<Funcionario> Subordinados { get; set; } = [];

    public ICollection<MarcacaoPonto> Marcacoes { get; set; } = [];

    public ICollection<SolicitacaoAjuste> SolicitacoesAjuste { get; set; } = [];

    public ICollection<Ausencia> Ausencias { get; set; } = [];
}
