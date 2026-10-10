using backend.Data;
using backend.Domain;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class FechamentoPeriodoService(PontoDbContext db, TimeProvider relogio)
{
    public Task<bool> EstaFechadoAsync(string codigoPais, Competencia competencia, CancellationToken ct) =>
        db.FechamentosPeriodo.AnyAsync(fechamento =>
            fechamento.CodigoPais == codigoPais &&
            fechamento.Ano == competencia.Ano &&
            fechamento.Mes == competencia.Mes &&
            fechamento.Status == StatusFechamento.Fechado, ct);

    public async Task GarantirAbertoAsync(string codigoPais, DateOnly dataJornada, CancellationToken ct)
    {
        var competencia = Competencia.De(dataJornada);
        if (await EstaFechadoAsync(codigoPais, competencia, ct))
        {
            throw new RegraNegocioException(
                $"O período {competencia} de {codigoPais} está fechado e não aceita alterações.");
        }
    }

    public async Task<FechamentoPeriodoResponse> FecharAsync(
        string codigoPais,
        Competencia competencia,
        Guid fechadoPorId,
        CancellationToken ct)
    {
        var codigo = codigoPais.Trim().ToUpperInvariant();
        if (!await db.Paises.AnyAsync(pais => pais.Codigo == codigo, ct))
        {
            throw new RecursoNaoEncontradoException($"País '{codigo}' não encontrado.");
        }

        var fechamento = await db.FechamentosPeriodo.FirstOrDefaultAsync(f =>
            f.CodigoPais == codigo && f.Ano == competencia.Ano && f.Mes == competencia.Mes, ct);

        if (fechamento is null)
        {
            fechamento = new FechamentoPeriodo { CodigoPais = codigo, Ano = competencia.Ano, Mes = competencia.Mes };
            db.FechamentosPeriodo.Add(fechamento);
        }

        fechamento.Fechar(fechadoPorId, relogio.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync(ct);

        return FechamentoPeriodoResponse.De(fechamento);
    }
}
