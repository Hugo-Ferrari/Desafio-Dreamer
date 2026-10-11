using System.ComponentModel.DataAnnotations;
using backend.Models;
using backend.Models.Enums;

namespace backend.Dtos;

public record CriarSolicitacaoAjusteRequest(
    TipoSolicitacaoAjuste TipoSolicitacao,
    [Required, MaxLength(2000)] string Motivo,
    Guid? MarcacaoAlvoId = null,
    TipoMarcacao? TipoMarcacaoProposto = null,
    DateTimeOffset? OcorridoEmProposto = null,
    string? FusoHorarioProposto = null);

public record AvaliarSolicitacaoAjusteRequest([MaxLength(2000)] string? Comentario = null);

public record SolicitacaoAjusteResponse(
    Guid Id,
    Guid FuncionarioId,
    string NomeFuncionario,
    TipoSolicitacaoAjuste TipoSolicitacao,
    Guid? MarcacaoAlvoId,
    TipoMarcacao? TipoMarcacaoProposto,
    DateTime? OcorridoEmUtcProposto,
    string? FusoHorarioProposto,
    string Motivo,
    StatusSolicitacaoAjuste Status,
    Guid? AvaliadorId,
    DateTime? AvaliadaEmUtc,
    string? ComentarioAvaliacao,
    DateTime CriadaEmUtc)
{
    public static SolicitacaoAjusteResponse De(SolicitacaoAjuste solicitacao) => new(
        solicitacao.Id,
        solicitacao.FuncionarioId,
        solicitacao.Funcionario.Nome,
        solicitacao.TipoSolicitacao,
        solicitacao.MarcacaoAlvoId,
        solicitacao.TipoMarcacaoProposto,
        solicitacao.OcorridoEmUtcProposto,
        solicitacao.FusoHorarioProposto,
        solicitacao.Motivo,
        solicitacao.Status,
        solicitacao.AvaliadorId,
        solicitacao.AvaliadaEmUtc,
        solicitacao.ComentarioAvaliacao,
        solicitacao.CriadaEmUtc);
}
