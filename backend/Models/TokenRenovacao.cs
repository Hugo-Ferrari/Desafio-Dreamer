namespace backend.Models;

public class TokenRenovacao
{
    public Guid Id { get; set; }

    public Guid FuncionarioId { get; set; }

    public Funcionario Funcionario { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;

    public DateTime CriadoEmUtc { get; set; }

    public DateTime ExpiraEmUtc { get; set; }

    public DateTime? RevogadoEmUtc { get; set; }

    public bool EstaAtivo(DateTime agoraUtc) => RevogadoEmUtc is null && ExpiraEmUtc > agoraUtc;

    public void Revogar(DateTime agoraUtc) => RevogadoEmUtc ??= agoraUtc;
}
