using System.ComponentModel.DataAnnotations;

namespace backend.Dtos;

public record LoginRequest([Required, EmailAddress] string Email, [Required] string Senha);

public record AlterarSenhaInicialRequest([Required, MinLength(12), MaxLength(128)] string NovaSenha);

public record TokenRenovacaoRequest([Required] string TokenRenovacao);

public record TokensResponse(string TokenAcesso, DateTime ExpiraEmUtc, string TokenRenovacao);
