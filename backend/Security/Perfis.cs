using backend.Models.Enums;

namespace backend.Security;

public static class Perfis
{
    public const string Gestor = nameof(PerfilFuncionario.Gestor);
    public const string RH = nameof(PerfilFuncionario.RH);
    public const string GestorOuRH = $"{Gestor},{RH}";
}
