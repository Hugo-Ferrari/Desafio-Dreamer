using backend.Exceptions;

namespace backend.Domain;

public static class FusoHorario
{
    public static TimeZoneInfo Obter(string identificador)
    {
        if (TimeZoneInfo.TryFindSystemTimeZoneById(identificador, out var fuso))
        {
            return fuso;
        }

        throw new RegraNegocioException(
            $"Fuso horário '{identificador}' inválido. Use um identificador IANA, como America/Sao_Paulo.");
    }

    public static int DeslocamentoEmMinutos(TimeZoneInfo fuso, DateTime instanteUtc) =>
        (int)fuso.GetUtcOffset(instanteUtc).TotalMinutes;

    public static DateOnly DataLocal(TimeZoneInfo fuso, DateTime instanteUtc) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(instanteUtc, fuso));
}
