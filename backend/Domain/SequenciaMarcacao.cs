using backend.Models;
using backend.Models.Enums;

namespace backend.Domain;

public static class SequenciaMarcacao
{
    public static readonly TimeSpan DuracaoMaximaJornada = TimeSpan.FromHours(16);

    public static EstadoJornada Estado(MarcacaoPonto? ultimaMarcacao, DateTime instanteUtc) => ultimaMarcacao switch
    {
        null => EstadoJornada.ForaDeJornada,
        _ when JornadaAbandonada(ultimaMarcacao, instanteUtc) => EstadoJornada.ForaDeJornada,
        { Tipo: TipoMarcacao.Saida } => EstadoJornada.ForaDeJornada,
        { Tipo: TipoMarcacao.InicioIntervalo } => EstadoJornada.EmIntervalo,
        _ => EstadoJornada.Trabalhando
    };

    public static IReadOnlyList<TipoMarcacao> ProximosTiposPermitidos(MarcacaoPonto? ultimaMarcacao, DateTime instanteUtc) =>
        Estado(ultimaMarcacao, instanteUtc) switch
        {
            EstadoJornada.ForaDeJornada => [TipoMarcacao.Entrada],
            EstadoJornada.Trabalhando => [TipoMarcacao.InicioIntervalo, TipoMarcacao.Saida],
            EstadoJornada.EmIntervalo => [TipoMarcacao.FimIntervalo],
            _ => []
        };

    private static bool JornadaAbandonada(MarcacaoPonto ultimaMarcacao, DateTime instanteUtc) =>
        instanteUtc - ultimaMarcacao.OcorridoEmUtc > DuracaoMaximaJornada;
}
