using backend.Data;
using backend.Domain;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class EspelhoPontoService(PontoDbContext db, FechamentoPeriodoService fechamentos, TimeProvider relogio)
{
    private const int MinutosTrabalhadosQueExigemIntervalo = 6 * 60;

    private record DadosDia(DateOnly Data, IReadOnlyList<MarcacaoPonto> Marcacoes, string? Feriado, Ausencia? Ausencia);

    public async Task<EspelhoMensalResponse> GerarAsync(Guid funcionarioId, Competencia competencia, CancellationToken ct)
    {
        var funcionario = await db.Funcionarios
            .AsNoTracking()
            .Include(f => f.JornadaContratual)
            .FirstOrDefaultAsync(f => f.Id == funcionarioId, ct)
            ?? throw new RecursoNaoEncontradoException("Funcionário não encontrado.");

        var inicio = competencia.PrimeiroDia;
        var fim = competencia.UltimoDia;

        var marcacoesPorDia = (await db.MarcacoesPonto
                .AsNoTracking()
                .Where(m => m.FuncionarioId == funcionarioId
                    && !m.Anulada
                    && m.DataJornadaLocal >= inicio
                    && m.DataJornadaLocal <= fim)
                .OrderBy(m => m.OcorridoEmUtc)
                .ToListAsync(ct))
            .ToLookup(m => m.DataJornadaLocal);

        var feriados = await db.Feriados
            .AsNoTracking()
            .Where(f => f.CodigoPais == funcionario.CodigoPais && f.Data >= inicio && f.Data <= fim)
            .ToDictionaryAsync(f => f.Data, f => f.Nome, ct);

        var ausencias = await db.Ausencias
            .AsNoTracking()
            .Where(a => a.FuncionarioId == funcionarioId && a.DataInicio <= fim && a.DataFim >= inicio)
            .ToListAsync(ct);

        var periodoFechado = await fechamentos.EstaFechadoAsync(funcionario.CodigoPais, competencia, ct);
        var hoje = FusoHorario.DataLocal(FusoHorario.Obter(funcionario.FusoHorarioPadrao), relogio.GetUtcNow().UtcDateTime);

        var dias = Enumerable.Range(0, fim.Day)
            .Select(inicio.AddDays)
            .Select(data => new DadosDia(
                data,
                marcacoesPorDia[data].ToList(),
                feriados.GetValueOrDefault(data),
                ausencias.FirstOrDefault(a => a.DataInicio <= data && a.DataFim >= data)))
            .Select(dia => MontarDia(dia, funcionario.JornadaContratual, hoje))
            .ToList();

        return new EspelhoMensalResponse(
            funcionario.Id,
            funcionario.Nome,
            funcionario.CodigoPais,
            competencia.ToString(),
            periodoFechado,
            Totalizar(dias),
            dias);
    }

    private static DiaEspelhoResponse MontarDia(DadosDia dia, JornadaContratual jornada, DateOnly hoje)
    {
        var resumo = CalculadoraJornada.Calcular(dia.Marcacoes);
        var situacao = Classificar(dia, resumo, hoje);
        var minutosEsperados = EhDiaUtil(dia) && situacao != SituacaoDia.Pendente ? jornada.MinutosDiarios : 0;
        var intervaloAbaixoDoMinimo = resumo.MinutosTrabalhados > MinutosTrabalhadosQueExigemIntervalo
            && resumo.MinutosIntervalo < jornada.MinutosIntervaloMinimo;

        return new DiaEspelhoResponse(
            dia.Data,
            situacao,
            DescreverObservacao(dia),
            minutosEsperados,
            resumo.MinutosTrabalhados,
            resumo.MinutosIntervalo,
            resumo.MinutosTrabalhados - minutosEsperados,
            intervaloAbaixoDoMinimo,
            dia.Marcacoes.Select(MarcacaoResponse.De).ToList());
    }

    private static SituacaoDia Classificar(DadosDia dia, ResumoJornada resumo, DateOnly hoje) => dia switch
    {
        { Marcacoes.Count: > 0 } => resumo.Completa ? SituacaoDia.Regular : SituacaoDia.Incompleto,
        { Ausencia: not null } => SituacaoDia.Ausencia,
        { Feriado: not null } => SituacaoDia.Feriado,
        _ when EhFimDeSemana(dia.Data) => SituacaoDia.FimDeSemana,
        _ when dia.Data >= hoje => SituacaoDia.Pendente,
        _ => SituacaoDia.Falta
    };

    private static string? DescreverObservacao(DadosDia dia) =>
        dia.Feriado ?? (dia.Ausencia is { } ausencia ? $"{ausencia.Tipo} {ausencia.Observacoes}".Trim() : null);

    private static bool EhDiaUtil(DadosDia dia) => !EhFimDeSemana(dia.Data) && dia.Feriado is null && dia.Ausencia is null;

    private static bool EhFimDeSemana(DateOnly data) => data.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    private static TotaisEspelhoResponse Totalizar(IReadOnlyCollection<DiaEspelhoResponse> dias) => new(
        dias.Sum(dia => dia.MinutosEsperados),
        dias.Sum(dia => dia.MinutosTrabalhados),
        dias.Sum(dia => dia.SaldoMinutos),
        dias.Count(dia => dia.Situacao == SituacaoDia.Incompleto),
        dias.Count(dia => dia.Situacao == SituacaoDia.Falta));
}
