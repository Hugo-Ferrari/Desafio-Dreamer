using System.ComponentModel.DataAnnotations;
using backend.Models;
using backend.Models.Enums;

namespace backend.Dtos;

public record CriarFuncionarioRequest(
    [Required, StringLength(150, MinimumLength = 1)] string Nome,
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, MinLength(8), MaxLength(128)] string SenhaInicial,
    PerfilFuncionario Perfil,
    [Required, StringLength(2, MinimumLength = 2)] string CodigoPais,
    [Required] Guid JornadaContratualId,
    Guid? GestorId = null,
    [StringLength(100)] string? FusoHorarioPadrao = null);

public record PaisOpcaoResponse(string Codigo, string Nome, string FusoHorarioPadrao);

public record GestorOpcaoResponse(Guid Id, string Nome, PerfilFuncionario Perfil);

public record OpcoesCadastroFuncionarioResponse(
    IReadOnlyList<PaisOpcaoResponse> Paises,
    IReadOnlyList<JornadaContratualResponse> JornadasContratuais,
    IReadOnlyList<GestorOpcaoResponse> Gestores);

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
