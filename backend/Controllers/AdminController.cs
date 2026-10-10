using System.ComponentModel.DataAnnotations;
using backend.Domain;
using backend.Dtos;
using backend.Security;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Perfis.RH)]
public class AdminController(
    FuncionarioService funcionarios,
    FechamentoPeriodoService fechamentos,
    RelatorioService relatorios) : ControllerBase
{
    [HttpGet("funcionarios")]
    public Task<IReadOnlyList<FuncionarioResponse>> ListarFuncionarios(CancellationToken ct) =>
        funcionarios.ListarAsync(ct);

    [HttpPost("periodos/{competencia}/fechar")]
    public Task<FechamentoPeriodoResponse> FecharPeriodo(
        Competencia competencia,
        [FromQuery, Required, StringLength(2, MinimumLength = 2)] string codigoPais,
        CancellationToken ct) =>
        fechamentos.FecharAsync(codigoPais, competencia, User.ObterUsuarioAtual().Id, ct);

    [HttpGet("relatorios/{competencia}.csv")]
    [Produces("text/csv")]
    public async Task<FileContentResult> GerarRelatorio(
        Competencia competencia,
        [FromQuery] string? codigoPais,
        CancellationToken ct)
    {
        var csv = await relatorios.GerarCsvMensalAsync(competencia, codigoPais, ct);
        return File(csv, "text/csv; charset=utf-8", $"relatorio-{competencia}.csv");
    }
}
