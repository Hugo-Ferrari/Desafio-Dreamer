using backend.Data;
using backend.Domain;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public record ResultadoRegistroMarcacao(MarcacaoResponse Marcacao, bool Criada);

public class MarcacaoService(PontoDbContext db, FabricaMarcacao fabrica, TimeProvider relogio)
{
    private const int DiasPadraoConsulta = 30;
    private static readonly TimeSpan ToleranciaRelogioCliente = TimeSpan.FromMinutes(2);

    public async Task<ResultadoRegistroMarcacao> RegistrarAsync(
        Guid funcionarioId,
        RegistrarMarcacaoRequest request,
        CancellationToken ct)
    {
        if (await BuscarPorChaveIdempotenciaAsync(funcionarioId, request.ChaveIdempotencia, ct) is { } existente)
        {
            return new ResultadoRegistroMarcacao(MarcacaoResponse.De(existente), Criada: false);
        }

        var funcionario = await db.Funcionarios.AsNoTracking().FirstOrDefaultAsync(f => f.Id == funcionarioId, ct)
            ?? throw new RecursoNaoEncontradoException("Funcionário não encontrado.");

        var ocorridoEmUtc = DefinirInstante(request, AgoraUtc);
        var ultimaMarcacao = await BuscarUltimaMarcacaoValidaAsync(funcionarioId, ct);
        ValidarSequencia(ultimaMarcacao, request.Tipo, ocorridoEmUtc);

        var marcacao = await fabrica.CriarAsync(new DadosNovaMarcacao(
            funcionarioId,
            funcionario.CodigoPais,
            request.Tipo,
            ocorridoEmUtc,
            request.FusoHorarioId ?? funcionario.FusoHorarioPadrao,
            request.ModalidadeTrabalho,
            request.OrigemRegistro,
            request.ChaveIdempotencia), ct);

        await db.SaveChangesAsync(ct);
        return new ResultadoRegistroMarcacao(MarcacaoResponse.De(marcacao), Criada: true);
    }

    public async Task<IReadOnlyList<MarcacaoResponse>> ListarAsync(
        Guid funcionarioId,
        DateOnly? de,
        DateOnly? ate,
        CancellationToken ct)
    {
        var fim = ate ?? await ObterDataLocalAtualAsync(funcionarioId, ct);
        var inicio = de ?? fim.AddDays(-DiasPadraoConsulta);

        if (inicio > fim)
        {
            throw new RegraNegocioException("A data inicial deve ser anterior ou igual à data final.");
        }

        var marcacoes = await db.MarcacoesPonto
            .AsNoTracking()
            .Where(m => m.FuncionarioId == funcionarioId && m.DataJornadaLocal >= inicio && m.DataJornadaLocal <= fim)
            .OrderBy(m => m.OcorridoEmUtc)
            .ToListAsync(ct);

        return marcacoes.Select(MarcacaoResponse.De).ToList();
    }

    public async Task<StatusJornadaResponse> ObterStatusAsync(Guid funcionarioId, CancellationToken ct)
    {
        var agora = AgoraUtc;
        var ultimaMarcacao = await BuscarUltimaMarcacaoValidaAsync(funcionarioId, ct);
        var estado = SequenciaMarcacao.Estado(ultimaMarcacao, agora);
        var proximasPermitidas = SequenciaMarcacao.ProximosTiposPermitidos(ultimaMarcacao, agora);

        if (ultimaMarcacao is null || estado == EstadoJornada.ForaDeJornada)
        {
            var ultima = ultimaMarcacao is null ? null : MarcacaoResponse.De(ultimaMarcacao);
            return new StatusJornadaResponse(estado, null, 0, ultima, proximasPermitidas);
        }

        var marcacoesDaJornada = await db.MarcacoesPonto
            .AsNoTracking()
            .Where(m => m.FuncionarioId == funcionarioId && m.DataJornadaLocal == ultimaMarcacao.DataJornadaLocal)
            .ToListAsync(ct);

        var resumo = CalculadoraJornada.Calcular(marcacoesDaJornada, agora);

        return new StatusJornadaResponse(
            estado,
            ultimaMarcacao.DataJornadaLocal,
            resumo.MinutosTrabalhados,
            MarcacaoResponse.De(ultimaMarcacao),
            proximasPermitidas);
    }

    private async Task<DateOnly> ObterDataLocalAtualAsync(Guid funcionarioId, CancellationToken ct)
    {
        var fusoPadrao = await db.Funcionarios
            .Where(f => f.Id == funcionarioId)
            .Select(f => f.FusoHorarioPadrao)
            .FirstOrDefaultAsync(ct)
            ?? throw new RecursoNaoEncontradoException("Funcionário não encontrado.");

        return FusoHorario.DataLocal(FusoHorario.Obter(fusoPadrao), AgoraUtc);
    }

    private Task<MarcacaoPonto?> BuscarUltimaMarcacaoValidaAsync(Guid funcionarioId, CancellationToken ct) =>
        db.MarcacoesPonto
            .AsNoTracking()
            .Where(m => m.FuncionarioId == funcionarioId && !m.Anulada)
            .OrderByDescending(m => m.OcorridoEmUtc)
            .FirstOrDefaultAsync(ct);

    private async Task<MarcacaoPonto?> BuscarPorChaveIdempotenciaAsync(
        Guid funcionarioId,
        Guid? chaveIdempotencia,
        CancellationToken ct)
    {
        if (chaveIdempotencia is null)
        {
            return null;
        }

        var existente = await db.MarcacoesPonto
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ChaveIdempotencia == chaveIdempotencia, ct);

        if (existente is not null && existente.FuncionarioId != funcionarioId)
        {
            throw new ConflitoException("A chave de idempotência informada já foi utilizada.");
        }

        return existente;
    }

    private static DateTime DefinirInstante(RegistrarMarcacaoRequest request, DateTime agoraUtc)
    {
        switch (request.OrigemRegistro)
        {
            case OrigemRegistro.Ajuste:
                throw new RegraNegocioException(
                    "Marcações de ajuste só são criadas pela aprovação de uma solicitação de ajuste.");

            case OrigemRegistro.Offline:
                var ocorridoEmUtc = request.OcorridoEm?.UtcDateTime
                    ?? throw new RegraNegocioException("Marcações offline precisam informar o campo OcorridoEm.");

                if (ocorridoEmUtc > agoraUtc + ToleranciaRelogioCliente)
                {
                    throw new RegraNegocioException("A marcação não pode estar no futuro.");
                }

                return ocorridoEmUtc;

            default:
                return agoraUtc;
        }
    }

    private static void ValidarSequencia(MarcacaoPonto? ultimaMarcacao, TipoMarcacao tipo, DateTime ocorridoEmUtc)
    {
        if (ultimaMarcacao is not null && ocorridoEmUtc <= ultimaMarcacao.OcorridoEmUtc)
        {
            throw new RegraNegocioException("A marcação precisa ser posterior à última marcação registrada.");
        }

        var permitidas = SequenciaMarcacao.ProximosTiposPermitidos(ultimaMarcacao, ocorridoEmUtc);
        if (!permitidas.Contains(tipo))
        {
            throw new RegraNegocioException(
                $"Marcação '{tipo}' não permitida agora. Próximas permitidas: {string.Join(", ", permitidas)}.");
        }
    }

    private DateTime AgoraUtc => relogio.GetUtcNow().UtcDateTime;
}
