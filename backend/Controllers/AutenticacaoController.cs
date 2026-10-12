using backend.Dtos;
using backend.Security;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/autenticacao")]
public class AutenticacaoController(AutenticacaoService autenticacao) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public Task<TokensResponse> Entrar(LoginRequest request, CancellationToken ct) =>
        autenticacao.EntrarAsync(request, ct);

    [HttpPost("renovar")]
    [AllowAnonymous]
    public Task<TokensResponse> Renovar(TokenRenovacaoRequest request, CancellationToken ct) =>
        autenticacao.RenovarAsync(request.TokenRenovacao, ct);

    [HttpPost("sair")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Sair(TokenRenovacaoRequest request, CancellationToken ct)
    {
        await autenticacao.SairAsync(request.TokenRenovacao, ct);
        return NoContent();
    }

    [HttpPost("alterar-senha-inicial")]
    [Authorize(Policy = GeradorTokens.PoliticaTrocaSenhaInicial)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AlterarSenhaInicial(AlterarSenhaInicialRequest request, CancellationToken ct)
    {
        await autenticacao.AlterarSenhaInicialAsync(User.ObterUsuarioAtual().Id, request, ct);
        return NoContent();
    }
}
