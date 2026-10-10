using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/autenticacao")]
[AllowAnonymous]
public class AutenticacaoController(AutenticacaoService autenticacao) : ControllerBase
{
    [HttpPost("login")]
    public Task<TokensResponse> Entrar(LoginRequest request, CancellationToken ct) =>
        autenticacao.EntrarAsync(request, ct);

    [HttpPost("renovar")]
    public Task<TokensResponse> Renovar(TokenRenovacaoRequest request, CancellationToken ct) =>
        autenticacao.RenovarAsync(request.TokenRenovacao, ct);

    [HttpPost("sair")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Sair(TokenRenovacaoRequest request, CancellationToken ct)
    {
        await autenticacao.SairAsync(request.TokenRenovacao, ct);
        return NoContent();
    }
}
