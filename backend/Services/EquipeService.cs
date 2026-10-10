using backend.Data;
using backend.Domain;
using backend.Dtos;
using backend.Security;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class EquipeService(
    PontoDbContext db,
    FuncionarioService funcionarios,
    EspelhoPontoService espelhos,
    TimeProvider relogio)
{
    public async Task<IReadOnlyList<MembroEquipeStatusResponse>> ObterStatusAsync(UsuarioAtual usuario, CancellationToken ct)
    {
        var membros = await db.Funcionarios.AsNoTracking().VisiveisPara(usuario).Where(f => f.Ativo && f.Id != usuario.Id).OrderBy(f => f.Nome).Select(f => new
            {
                f.Id,
                f.Nome,
                f.CodigoPais,
                UltimaMarcacao = f.Marcacoes
                    .Where(m => !m.Anulada)
                    .OrderByDescending(m => m.OcorridoEmUtc)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var agora = relogio.GetUtcNow().UtcDateTime;

        return membros
            .Select(membro => new MembroEquipeStatusResponse(
                membro.Id,
                membro.Nome,
                membro.CodigoPais,
                SequenciaMarcacao.Estado(membro.UltimaMarcacao, agora),
                membro.UltimaMarcacao is null ? null : MarcacaoResponse.De(membro.UltimaMarcacao)))
            .ToList();
    }

    public async Task<EspelhoMensalResponse> ObterEspelhoDoMembroAsync(
        UsuarioAtual usuario,
        Guid funcionarioId,
        Competencia competencia,
        CancellationToken ct)
    {
        await funcionarios.GarantirAcessoAsync(usuario, funcionarioId, ct);
        return await espelhos.GerarAsync(funcionarioId, competencia, ct);
    }
}
