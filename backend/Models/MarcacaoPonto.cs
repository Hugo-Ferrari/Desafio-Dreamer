using backend.Exceptions;
using backend.Models.Enums;

namespace backend.Models;

public class MarcacaoPonto
{
    public Guid Id { get; set; }

    public Guid FuncionarioId { get; set; }

    public Funcionario Funcionario { get; set; } = null!;

    public TipoMarcacao Tipo { get; set; }

    public DateTime OcorridoEmUtc { get; set; }

    public DateTime RegistradoEmUtc { get; set; }

    public string FusoHorarioId { get; set; } = string.Empty;

    public int DeslocamentoUtcMinutos { get; set; }

    public DateOnly DataJornadaLocal { get; set; }

    public ModalidadeTrabalho ModalidadeTrabalho { get; set; }

    public OrigemRegistro OrigemRegistro { get; set; }

    public bool Anulada { get; set; }

    public Guid? SubstituiMarcacaoId { get; set; }

    public MarcacaoPonto? SubstituiMarcacao { get; set; }

    public Guid? ChaveIdempotencia { get; set; }

    public void Anular()
    {
        if (Anulada)
        {
            throw new RegraNegocioException("A marcação já está anulada.");
        }

        Anulada = true;
    }
}
