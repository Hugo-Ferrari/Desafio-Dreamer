using backend.Data;
using backend.Domain;
using backend.Models;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public record DadosNovaMarcacao(
    Guid FuncionarioId,
    string CodigoPais,
    TipoMarcacao Tipo,
    DateTime OcorridoEmUtc,
    string FusoHorarioId,
    ModalidadeTrabalho ModalidadeTrabalho,
    OrigemRegistro OrigemRegistro,
    Guid? ChaveIdempotencia = null,
    Guid? SubstituiMarcacaoId = null);

public class FabricaMarcacao(PontoDbContext db, FechamentoPeriodoService fechamentos, TimeProvider relogio)
{
    public async Task<MarcacaoPonto> CriarAsync(DadosNovaMarcacao dados, CancellationToken ct)
    {
        var fuso = FusoHorario.Obter(dados.FusoHorarioId);
        var dataJornada = await ResolverDataJornadaAsync(dados, fuso, ct);

        await fechamentos.GarantirAbertoAsync(dados.CodigoPais, dataJornada, ct);

        var marcacao = new MarcacaoPonto
        {
            FuncionarioId = dados.FuncionarioId,
            Tipo = dados.Tipo,
            OcorridoEmUtc = dados.OcorridoEmUtc,
            RegistradoEmUtc = relogio.GetUtcNow().UtcDateTime,
            FusoHorarioId = dados.FusoHorarioId,
            DeslocamentoUtcMinutos = FusoHorario.DeslocamentoEmMinutos(fuso, dados.OcorridoEmUtc),
            DataJornadaLocal = dataJornada,
            ModalidadeTrabalho = dados.ModalidadeTrabalho,
            OrigemRegistro = dados.OrigemRegistro,
            ChaveIdempotencia = dados.ChaveIdempotencia,
            SubstituiMarcacaoId = dados.SubstituiMarcacaoId
        };

        db.MarcacoesPonto.Add(marcacao);
        return marcacao;
    }

    private async Task<DateOnly> ResolverDataJornadaAsync(DadosNovaMarcacao dados, TimeZoneInfo fuso, CancellationToken ct)
    {
        if (dados.Tipo != TipoMarcacao.Entrada)
        {
            var dataDaEntrada = await db.MarcacoesPonto
                .Where(m => m.FuncionarioId == dados.FuncionarioId
                    && !m.Anulada
                    && m.Tipo == TipoMarcacao.Entrada
                    && m.OcorridoEmUtc <= dados.OcorridoEmUtc)
                .OrderByDescending(m => m.OcorridoEmUtc)
                .Select(m => (DateOnly?)m.DataJornadaLocal)
                .FirstOrDefaultAsync(ct);

            if (dataDaEntrada is { } data)
            {
                return data;
            }
        }

        return FusoHorario.DataLocal(fuso, dados.OcorridoEmUtc);
    }
}
