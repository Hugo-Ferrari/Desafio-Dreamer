using backend.Models;
using backend.Models.Enums;
using backend.Security;

namespace backend.Data;

public static class ConsultasFuncionario
{
    public static IQueryable<Funcionario> VisiveisPara(this IQueryable<Funcionario> funcionarios, UsuarioAtual usuario) =>
        usuario.Perfil switch
        {
            PerfilFuncionario.RH => funcionarios,
            PerfilFuncionario.Gestor => funcionarios.Where(funcionario =>
                funcionario.Id == usuario.Id || funcionario.GestorId == usuario.Id),
            _ => funcionarios.Where(funcionario => funcionario.Id == usuario.Id)
        };
}
