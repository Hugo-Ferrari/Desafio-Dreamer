using System.ComponentModel.DataAnnotations;

namespace backend.Dtos;

public record LoginRequest([Required, EmailAddress] string Email, [Required] string Senha);

public record TokenRenovacaoRequest([Required] string TokenRenovacao);

public record TokensResponse(string TokenAcesso, DateTime ExpiraEmUtc, string TokenRenovacao);
