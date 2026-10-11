using backend.Domain;
using backend.Models;
using backend.Models.Enums;

namespace backend.Dtos;

public record FechamentoPeriodoResponse(
    Guid Id,
    string CodigoPais,
    string Competencia,
    StatusFechamento Status,
    Guid? FechadoPorId,
    DateTime? FechadoEmUtc)
{
    public static FechamentoPeriodoResponse De(FechamentoPeriodo fechamento) => new(
        fechamento.Id,
        fechamento.CodigoPais,
        new Competencia(fechamento.Ano, fechamento.Mes).ToString(),
        fechamento.Status,
        fechamento.FechadoPorId,
        fechamento.FechadoEmUtc);
}
