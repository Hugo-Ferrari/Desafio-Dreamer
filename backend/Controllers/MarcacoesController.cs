using backend.Dtos;
using backend.Security;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/marcacoes")]
public class MarcacoesController(MarcacaoService marcacoes) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<MarcacaoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<MarcacaoResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Registrar(RegistrarMarcacaoRequest request, CancellationToken ct)
    {
        var resultado = await marcacoes.RegistrarAsync(User.ObterUsuarioAtual().Id, request, ct);

        return resultado.Criada
            ? StatusCode(StatusCodes.Status201Created, resultado.Marcacao)
            : Ok(resultado.Marcacao);
    }

    [HttpGet]
    public Task<IReadOnlyList<MarcacaoResponse>> Listar(
        [FromQuery] DateOnly? de,
        [FromQuery] DateOnly? ate,
        CancellationToken ct) =>
        marcacoes.ListarAsync(User.ObterUsuarioAtual().Id, de, ate, ct);
}
