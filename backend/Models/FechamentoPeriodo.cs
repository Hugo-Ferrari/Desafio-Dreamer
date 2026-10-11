using backend.Exceptions;
using backend.Models.Enums;

namespace backend.Models;

public class FechamentoPeriodo
{
    public Guid Id { get; set; }

    public string CodigoPais { get; set; } = string.Empty;

    public Pais Pais { get; set; } = null!;

    public int Ano { get; set; }

    public int Mes { get; set; }

    public StatusFechamento Status { get; set; } = StatusFechamento.Aberto;

    public Guid? FechadoPorId { get; set; }

    public Funcionario? FechadoPor { get; set; }

    public DateTime? FechadoEmUtc { get; set; }

    public void Fechar(Guid fechadoPorId, DateTime agoraUtc)
    {
        if (Status == StatusFechamento.Fechado)
        {
            throw new ConflitoException($"O período {Ano:D4}-{Mes:D2} de {CodigoPais} já está fechado.");
        }

        Status = StatusFechamento.Fechado;
        FechadoPorId = fechadoPorId;
        FechadoEmUtc = agoraUtc;
    }
}
