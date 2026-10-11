using backend.Models;
using backend.Models.Enums;

namespace backend.Domain;

public record ResumoJornada(int MinutosTrabalhados, int MinutosIntervalo, bool Completa);

public static class CalculadoraJornada
{
    public static ResumoJornada Calcular(IEnumerable<MarcacaoPonto> marcacoes, DateTime? agoraUtc = null)
    {
        var validas = marcacoes
            .Where(marcacao => !marcacao.Anulada)
            .OrderBy(marcacao => marcacao.OcorridoEmUtc)
            .ToList();

        var trabalhado = TimeSpan.Zero;
        var intervalo = TimeSpan.Zero;
        DateTime? inicioTrabalho = null;
        DateTime? inicioIntervalo = null;

        foreach (var marcacao in validas)
        {
            switch (marcacao.Tipo)
            {
                case TipoMarcacao.Entrada:
                    inicioTrabalho = marcacao.OcorridoEmUtc;
                    break;

                case TipoMarcacao.InicioIntervalo:
                    trabalhado += DuracaoDesde(inicioTrabalho, marcacao.OcorridoEmUtc);
                    inicioTrabalho = null;
                    inicioIntervalo = marcacao.OcorridoEmUtc;
                    break;

                case TipoMarcacao.FimIntervalo:
                    intervalo += DuracaoDesde(inicioIntervalo, marcacao.OcorridoEmUtc);
                    inicioIntervalo = null;
                    inicioTrabalho = marcacao.OcorridoEmUtc;
                    break;

                case TipoMarcacao.Saida:
                    trabalhado += DuracaoDesde(inicioTrabalho, marcacao.OcorridoEmUtc);
                    inicioTrabalho = null;
                    break;
            }
        }

        if (agoraUtc is { } agora)
        {
            trabalhado += DuracaoDesde(inicioTrabalho, agora);
        }

        var completa = validas.Count > 0 && validas[^1].Tipo == TipoMarcacao.Saida;

        return new ResumoJornada((int)trabalhado.TotalMinutes, (int)intervalo.TotalMinutes, completa);
    }

    private static TimeSpan DuracaoDesde(DateTime? inicio, DateTime fim) =>
        inicio is { } valor && fim > valor ? fim - valor : TimeSpan.Zero;
}
