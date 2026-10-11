using backend.Models;
using backend.Models.Enums;

namespace backend.Dtos;

public record JornadaContratualResponse(
    Guid Id,
    string Nome,
    int MinutosDiarios,
    int MinutosSemanais,
    int MinutosIntervaloMinimo)
{
    public static JornadaContratualResponse De(JornadaContratual jornada) => new(
        jornada.Id,
        jornada.Nome,
        jornada.MinutosDiarios,
        jornada.MinutosSemanais,
        jornada.MinutosIntervaloMinimo);
}

public record FuncionarioResponse(
    Guid Id,
    string Nome,
    string Email,
    PerfilFuncionario Perfil,
    string CodigoPais,
    string FusoHorarioPadrao,
    JornadaContratualResponse JornadaContratual,
    Guid? GestorId,
    string? NomeGestor,
    bool Ativo)
{
    public static FuncionarioResponse De(Funcionario funcionario) => new(
        funcionario.Id,
        funcionario.Nome,
        funcionario.Email,
        funcionario.Perfil,
        funcionario.CodigoPais,
        funcionario.FusoHorarioPadrao,
        JornadaContratualResponse.De(funcionario.JornadaContratual),
        funcionario.GestorId,
        funcionario.Gestor?.Nome,
        funcionario.Ativo);
}
