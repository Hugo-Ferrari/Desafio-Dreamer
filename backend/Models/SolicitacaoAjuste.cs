using backend.Exceptions;
using backend.Models.Enums;

namespace backend.Models;

public class SolicitacaoAjuste
{
    public Guid Id { get; set; }

    public Guid FuncionarioId { get; set; }

    public Funcionario Funcionario { get; set; } = null!;

    public TipoSolicitacaoAjuste TipoSolicitacao { get; set; }

    public Guid? MarcacaoAlvoId { get; set; }

    public MarcacaoPonto? MarcacaoAlvo { get; set; }

    public TipoMarcacao? TipoMarcacaoProposto { get; set; }

    public DateTime? OcorridoEmUtcProposto { get; set; }

    public string? FusoHorarioProposto { get; set; }

    public string Motivo { get; set; } = string.Empty;

    public StatusSolicitacaoAjuste Status { get; set; } = StatusSolicitacaoAjuste.Pendente;

    public Guid? AvaliadorId { get; set; }

    public Funcionario? Avaliador { get; set; }

    public DateTime? AvaliadaEmUtc { get; set; }

    public string? ComentarioAvaliacao { get; set; }

    public DateTime CriadaEmUtc { get; set; }

    public void Aprovar(Guid avaliadorId, DateTime agoraUtc, string? comentario) =>
        Avaliar(StatusSolicitacaoAjuste.Aprovada, avaliadorId, agoraUtc, comentario);

    public void Rejeitar(Guid avaliadorId, DateTime agoraUtc, string? comentario) =>
        Avaliar(StatusSolicitacaoAjuste.Rejeitada, avaliadorId, agoraUtc, comentario);

    private void Avaliar(StatusSolicitacaoAjuste novoStatus, Guid avaliadorId, DateTime agoraUtc, string? comentario)
    {
        if (Status != StatusSolicitacaoAjuste.Pendente)
        {
            throw new RegraNegocioException("Apenas solicitações pendentes podem ser avaliadas.");
        }

        Status = novoStatus;
        AvaliadorId = avaliadorId;
        AvaliadaEmUtc = agoraUtc;
        ComentarioAvaliacao = comentario;
    }
}
