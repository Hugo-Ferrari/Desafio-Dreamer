using backend.Domain;

namespace backend.Dtos;

public record MembroEquipeStatusResponse(
    Guid FuncionarioId,
    string Nome,
    string CodigoPais,
    EstadoJornada Estado,
    MarcacaoResponse? UltimaMarcacao);
