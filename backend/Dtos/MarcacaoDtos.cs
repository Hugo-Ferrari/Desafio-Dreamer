using backend.Domain;
using backend.Models;
using backend.Models.Enums;

namespace backend.Dtos;

public record RegistrarMarcacaoRequest(
    TipoMarcacao Tipo,
    ModalidadeTrabalho ModalidadeTrabalho,
    OrigemRegistro OrigemRegistro,
    string? FusoHorarioId = null,
    DateTimeOffset? OcorridoEm = null,
    Guid? ChaveIdempotencia = null);

public record MarcacaoResponse(
    Guid Id,
    TipoMarcacao Tipo,
    DateTime OcorridoEmUtc,
    DateTimeOffset OcorridoEmLocal,
    DateTime RegistradoEmUtc,
    string FusoHorarioId,
    DateOnly DataJornadaLocal,
    ModalidadeTrabalho ModalidadeTrabalho,
    OrigemRegistro OrigemRegistro,
    bool Anulada,
    Guid? SubstituiMarcacaoId)
{
    public static MarcacaoResponse De(MarcacaoPonto marcacao) => new(
        marcacao.Id,
        marcacao.Tipo,
        marcacao.OcorridoEmUtc,
        new DateTimeOffset(marcacao.OcorridoEmUtc).ToOffset(TimeSpan.FromMinutes(marcacao.DeslocamentoUtcMinutos)),
        marcacao.RegistradoEmUtc,
        marcacao.FusoHorarioId,
        marcacao.DataJornadaLocal,
        marcacao.ModalidadeTrabalho,
        marcacao.OrigemRegistro,
        marcacao.Anulada,
        marcacao.SubstituiMarcacaoId);
}

public record StatusJornadaResponse(
    EstadoJornada Estado,
    DateOnly? DataJornadaLocal,
    int MinutosTrabalhadosNaJornada,
    MarcacaoResponse? UltimaMarcacao,
    IReadOnlyList<TipoMarcacao> ProximasMarcacoesPermitidas);
