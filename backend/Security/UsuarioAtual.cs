using System.Security.Claims;
using backend.Models.Enums;
using Microsoft.IdentityModel.JsonWebTokens;

namespace backend.Security;

public record UsuarioAtual(Guid Id, PerfilFuncionario Perfil);

public static class ClaimsPrincipalExtensions
{
    public static UsuarioAtual ObterUsuarioAtual(this ClaimsPrincipal usuario)
    {
        var id = usuario.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var perfil = usuario.FindFirstValue(GeradorTokens.ClaimPerfil);

        if (!Guid.TryParse(id, out var funcionarioId) || !Enum.TryParse<PerfilFuncionario>(perfil, out var perfilFuncionario))
        {
            throw new InvalidOperationException("O token de acesso não contém as claims esperadas.");
        }

        return new UsuarioAtual(funcionarioId, perfilFuncionario);
    }
}
