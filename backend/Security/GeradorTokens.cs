using System.Security.Cryptography;
using System.Text;
using backend.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace backend.Security;

public record TokenAcesso(string Token, DateTime ExpiraEmUtc);

public class GeradorTokens(IOptions<ConfiguracaoJwt> opcoes, TimeProvider relogio)
{
    public const string ClaimPerfil = "role";
    public const string ClaimTrocaSenhaInicial = "troca_senha_inicial";
    public const string PoliticaTrocaSenhaInicial = "TrocaSenhaInicial";

    private readonly ConfiguracaoJwt _configuracao = opcoes.Value;

    public TimeSpan ValidadeRenovacao => TimeSpan.FromDays(_configuracao.DiasValidadeRenovacao);

    public TokenAcesso GerarTokenAcesso(Funcionario funcionario)
    {
        var expiraEmUtc = relogio.GetUtcNow().UtcDateTime.AddMinutes(_configuracao.MinutosValidadeAcesso);

        var descritor = new SecurityTokenDescriptor
        {
            Issuer = _configuracao.Emissor,
            Audience = _configuracao.Audiencia,
            Expires = expiraEmUtc,
            SigningCredentials = new SigningCredentials(CriarChaveAssinatura(_configuracao.Chave), SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = funcionario.Id.ToString(),
                [JwtRegisteredClaimNames.Email] = funcionario.Email,
                [JwtRegisteredClaimNames.Name] = funcionario.Nome,
                [ClaimPerfil] = funcionario.Perfil.ToString()
            }
        };

        if (funcionario.DeveAlterarSenha)
        {
            descritor.Claims[ClaimTrocaSenhaInicial] = true;
        }

        return new TokenAcesso(new JsonWebTokenHandler().CreateToken(descritor), expiraEmUtc);
    }

    public static string GerarTokenRenovacao() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public static string CalcularHash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public static SymmetricSecurityKey CriarChaveAssinatura(string chave) => new(Encoding.UTF8.GetBytes(chave));
}
