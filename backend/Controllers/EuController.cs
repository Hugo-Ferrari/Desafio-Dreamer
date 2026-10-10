using backend.Dtos;
using backend.Security;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/eu")]
public class EuController(FuncionarioService funcionarios, MarcacaoService marcacoes) : ControllerBase
{
    [HttpGet]
    public Task<FuncionarioResponse> ObterPerfil(CancellationToken ct) =>
        funcionarios.ObterAsync(User.ObterUsuarioAtual().Id, ct);

    [HttpGet("status")]
    public Task<StatusJornadaResponse> ObterStatus(CancellationToken ct) =>
        marcacoes.ObterStatusAsync(User.ObterUsuarioAtual().Id, ct);
}
