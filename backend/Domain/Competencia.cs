using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace backend.Domain;

public readonly record struct Competencia(int Ano, int Mes) : IParsable<Competencia>
{
    private const string Formato = "yyyy-MM";

    public DateOnly PrimeiroDia => new(Ano, Mes, 1);

    public DateOnly UltimoDia => PrimeiroDia.AddMonths(1).AddDays(-1);

    public bool Contem(DateOnly data) => data.Year == Ano && data.Month == Mes;

    public static Competencia De(DateOnly data) => new(data.Year, data.Month);

    public override string ToString() => PrimeiroDia.ToString(Formato, CultureInfo.InvariantCulture);

    public static Competencia Parse(string s, IFormatProvider? provider) =>
        TryParse(s, provider, out var competencia)
            ? competencia
            : throw new FormatException($"Competência '{s}' inválida. Use o formato {Formato}.");

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Competencia resultado)
    {
        if (DateOnly.TryParseExact(s, Formato, CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
        {
            resultado = De(data);
            return true;
        }

        resultado = default;
        return false;
    }
}
