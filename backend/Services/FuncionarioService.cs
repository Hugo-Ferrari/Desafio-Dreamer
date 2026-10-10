using backend.Data;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using backend.Security;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class FuncionarioService(PontoDbContext db)
{
    public async Task<FuncionarioResponse> ObterAsync(Guid funcionarioId, CancellationToken ct)
    {
        var funcionario = await ConsultaComRelacionamentos().FirstOrDefaultAsync(f => f.Id == funcionarioId, ct)
            ?? throw new RecursoNaoEncontradoException("Funcionário não encontrado.");

        return FuncionarioResponse.De(funcionario);
    }

    public async Task<IReadOnlyList<FuncionarioResponse>> ListarAsync(CancellationToken ct)
    {
        var funcionarios = await ConsultaComRelacionamentos().OrderBy(f => f.Nome).ToListAsync(ct);
        return funcionarios.Select(FuncionarioResponse.De).ToList();
    }

    public async Task GarantirAcessoAsync(UsuarioAtual usuario, Guid funcionarioId, CancellationToken ct)
    {
        if (await db.Funcionarios.VisiveisPara(usuario).AnyAsync(f => f.Id == funcionarioId, ct))
        {
            return;
        }

        if (await db.Funcionarios.AnyAsync(f => f.Id == funcionarioId, ct))
        {
            throw new AcessoNegadoException("Você não tem acesso aos dados deste funcionário.");
        }

        throw new RecursoNaoEncontradoException("Funcionário não encontrado.");
    }

    private IQueryable<Funcionario> ConsultaComRelacionamentos() => db.Funcionarios
        .AsNoTracking()
        .Include(f => f.JornadaContratual)
        .Include(f => f.Gestor);
}
