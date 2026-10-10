using backend.Domain;
using backend.Dtos;
using backend.Security;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/equipe")]
[Authorize(Roles = Perfis.GestorOuRH)]
public class EquipeController(EquipeService equipe) : ControllerBase
{
    [HttpGet("status")]
    public Task<IReadOnlyList<MembroEquipeStatusResponse>> ObterStatus(CancellationToken ct) =>
        equipe.ObterStatusAsync(User.ObterUsuarioAtual(), ct);

    [HttpGet("{funcionarioId:guid}/espelhos/{competencia}")]
    public Task<EspelhoMensalResponse> ObterEspelho(Guid funcionarioId, Competencia competencia, CancellationToken ct) =>
        equipe.ObterEspelhoDoMembroAsync(User.ObterUsuarioAtual(), funcionarioId, competencia, ct);
}
