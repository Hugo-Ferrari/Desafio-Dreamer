using backend.Domain;

namespace backend.Dtos;

public record DiaEspelhoResponse(
    DateOnly Data,
    SituacaoDia Situacao,
    string? Observacao,
    int MinutosEsperados,
    int MinutosTrabalhados,
    int MinutosIntervalo,
    int SaldoMinutos,
    bool IntervaloAbaixoDoMinimo,
    IReadOnlyList<MarcacaoResponse> Marcacoes);

public record TotaisEspelhoResponse(
    int MinutosEsperados,
    int MinutosTrabalhados,
    int SaldoMinutos,
    int DiasIncompletos,
    int Faltas);

public record EspelhoMensalResponse(
    Guid FuncionarioId,
    string NomeFuncionario,
    string CodigoPais,
    string Competencia,
    bool PeriodoFechado,
    TotaisEspelhoResponse Totais,
    IReadOnlyList<DiaEspelhoResponse> Dias);
