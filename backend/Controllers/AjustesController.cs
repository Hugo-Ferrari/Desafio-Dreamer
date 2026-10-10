using backend.Dtos;
using backend.Models.Enums;
using backend.Security;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/ajustes")]
public class AjustesController(AjusteService ajustes) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<SolicitacaoAjusteResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Criar(CriarSolicitacaoAjusteRequest request, CancellationToken ct)
    {
        var solicitacao = await ajustes.CriarAsync(User.ObterUsuarioAtual().Id, request, ct);
        return StatusCode(StatusCodes.Status201Created, solicitacao);
    }

    [HttpGet]
    public Task<IReadOnlyList<SolicitacaoAjusteResponse>> Listar(
        [FromQuery] StatusSolicitacaoAjuste? status,
        CancellationToken ct) =>
        ajustes.ListarAsync(User.ObterUsuarioAtual(), status, ct);

    [HttpPost("{id:guid}/aprovar")]
    [Authorize(Roles = Perfis.GestorOuRH)]
    public Task<SolicitacaoAjusteResponse> Aprovar(Guid id, AvaliarSolicitacaoAjusteRequest request, CancellationToken ct) =>
        ajustes.AprovarAsync(User.ObterUsuarioAtual(), id, request.Comentario, ct);

    [HttpPost("{id:guid}/rejeitar")]
    [Authorize(Roles = Perfis.GestorOuRH)]
    public Task<SolicitacaoAjusteResponse> Rejeitar(Guid id, AvaliarSolicitacaoAjusteRequest request, CancellationToken ct) =>
        ajustes.RejeitarAsync(User.ObterUsuarioAtual(), id, request.Comentario, ct);
}
