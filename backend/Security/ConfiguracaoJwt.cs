using System.ComponentModel.DataAnnotations;

namespace backend.Security;

public class ConfiguracaoJwt
{
    public const string Secao = "Jwt";

    [Required]
    public string Emissor { get; init; } = string.Empty;

    [Required]
    public string Audiencia { get; init; } = string.Empty;

    [Required, MinLength(32)]
    public string Chave { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int MinutosValidadeAcesso { get; init; } = 15;

    [Range(1, 90)]
    public int DiasValidadeRenovacao { get; init; } = 7;
}
