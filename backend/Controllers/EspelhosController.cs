using backend.Domain;
using backend.Dtos;
using backend.Security;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/espelhos")]
public class EspelhosController(EspelhoPontoService espelhos) : ControllerBase
{
    [HttpGet("{competencia}")]
    public Task<EspelhoMensalResponse> Obter(Competencia competencia, CancellationToken ct) =>
        espelhos.GerarAsync(User.ObterUsuarioAtual().Id, competencia, ct);
}
