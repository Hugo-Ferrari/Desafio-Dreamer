using System.Text;
using backend.Data;
using backend.Domain;
using backend.Dtos;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class RelatorioService(PontoDbContext db, EspelhoPontoService espelhos)
{
    private const char Separador = ';';

    private static readonly string[] Cabecalho =
    [
        "Funcionario", "Email", "Pais", "Competencia", "MinutosEsperados",
        "MinutosTrabalhados", "SaldoMinutos", "DiasIncompletos", "Faltas"
    ];

    public async Task<byte[]> GerarCsvMensalAsync(Competencia competencia, string? codigoPais, CancellationToken ct)
    {
        var consulta = db.Funcionarios.AsNoTracking().Where(f => f.Ativo);

        if (!string.IsNullOrWhiteSpace(codigoPais))
        {
            var codigo = codigoPais.Trim().ToUpperInvariant();
            consulta = consulta.Where(f => f.CodigoPais == codigo);
        }

        var funcionarios = await consulta
            .OrderBy(f => f.Nome)
            .Select(f => new { f.Id, f.Email })
            .ToListAsync(ct);

        var csv = new StringBuilder().AppendLine(MontarLinha(Cabecalho));

        foreach (var funcionario in funcionarios)
        {
            var espelho = await espelhos.GerarAsync(funcionario.Id, competencia, ct);
            csv.AppendLine(MontarLinha(ValoresDaLinha(espelho, funcionario.Email)));
        }

        return [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(csv.ToString())];
    }

    private static IEnumerable<string> ValoresDaLinha(EspelhoMensalResponse espelho, string email) =>
    [
        espelho.NomeFuncionario,
        email,
        espelho.CodigoPais,
        espelho.Competencia,
        espelho.Totais.MinutosEsperados.ToString(),
        espelho.Totais.MinutosTrabalhados.ToString(),
        espelho.Totais.SaldoMinutos.ToString(),
        espelho.Totais.DiasIncompletos.ToString(),
        espelho.Totais.Faltas.ToString()
    ];

    private static string MontarLinha(IEnumerable<string> valores) => string.Join(Separador, valores.Select(Escapar));

    private static string Escapar(string valor) =>
        valor.IndexOfAny([Separador, '"', '\n', '\r']) >= 0
            ? $"\"{valor.Replace("\"", "\"\"")}\""
            : valor;
}
