using System.ComponentModel.DataAnnotations;
using backend.Data;
using backend.Domain;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using backend.Models.Enums;
using backend.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace backend.Services;

public class FuncionarioService(
    PontoDbContext db,
    IPasswordHasher<Funcionario> hasherSenha,
    TimeProvider relogio)
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

    public async Task<OpcoesCadastroFuncionarioResponse> ObterOpcoesCadastroAsync(CancellationToken ct)
    {
        var paises = await db.Paises
            .AsNoTracking()
            .OrderBy(pais => pais.Nome)
            .Select(pais => new PaisOpcaoResponse(pais.Codigo, pais.Nome, pais.FusoHorarioPadrao))
            .ToListAsync(ct);

        var jornadas = await db.JornadasContratuais
            .AsNoTracking()
            .OrderBy(jornada => jornada.Nome)
            .Select(jornada => new JornadaContratualResponse(
                jornada.Id,
                jornada.Nome,
                jornada.MinutosDiarios,
                jornada.MinutosSemanais,
                jornada.MinutosIntervaloMinimo))
            .ToListAsync(ct);

        var gestores = await db.Funcionarios
            .AsNoTracking()
            .Where(funcionario => funcionario.Ativo &&
                (funcionario.Perfil == PerfilFuncionario.Gestor || funcionario.Perfil == PerfilFuncionario.RH))
            .OrderBy(funcionario => funcionario.Nome)
            .Select(funcionario => new GestorOpcaoResponse(funcionario.Id, funcionario.Nome, funcionario.Perfil))
            .ToListAsync(ct);

        return new OpcoesCadastroFuncionarioResponse(paises, jornadas, gestores);
    }

    public async Task<FuncionarioResponse> CriarAsync(CriarFuncionarioRequest request, CancellationToken ct)
    {
        var nome = request.Nome.Trim();
        if (nome.Length is < 1 or > 150)
        {
            throw new RegraNegocioException("O nome deve ter entre 1 e 150 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(request.SenhaInicial) ||
            request.SenhaInicial.Length is < 8 or > 128)
        {
            throw new RegraNegocioException("A senha inicial deve ter entre 8 e 128 caracteres.");
        }

        if (request.Perfil is not (PerfilFuncionario.Colaborador or PerfilFuncionario.Gestor))
        {
            throw new RegraNegocioException("O RH pode cadastrar funcionários apenas como Colaborador ou Gestor.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (email.Length > 254 || !new EmailAddressAttribute().IsValid(email))
        {
            throw new RegraNegocioException("Informe um e-mail válido com até 254 caracteres.");
        }

        if (await db.Funcionarios.AnyAsync(
                funcionario => funcionario.Email.ToLower() == email,
                ct))
        {
            throw new ConflitoException("Já existe um funcionário cadastrado com este e-mail.");
        }

        var codigoPais = request.CodigoPais.Trim().ToUpperInvariant();
        var pais = await db.Paises.FirstOrDefaultAsync(item => item.Codigo == codigoPais, ct)
            ?? throw new RecursoNaoEncontradoException($"País '{codigoPais}' não encontrado.");

        var jornada = await db.JornadasContratuais
            .FirstOrDefaultAsync(item => item.Id == request.JornadaContratualId, ct)
            ?? throw new RecursoNaoEncontradoException("Jornada contratual não encontrada.");

        Funcionario? gestor = null;
        if (request.GestorId is { } gestorId)
        {
            gestor = await db.Funcionarios.FirstOrDefaultAsync(
                item => item.Id == gestorId &&
                    item.Ativo &&
                    (item.Perfil == PerfilFuncionario.Gestor || item.Perfil == PerfilFuncionario.RH),
                ct)
                ?? throw new RegraNegocioException("O gestor selecionado não existe ou não está ativo.");
        }

        var fusoHorario = string.IsNullOrWhiteSpace(request.FusoHorarioPadrao)
            ? pais.FusoHorarioPadrao
            : request.FusoHorarioPadrao.Trim();
        _ = FusoHorario.Obter(fusoHorario);

        var funcionario = new Funcionario
        {
            Nome = nome,
            Email = email,
            Perfil = request.Perfil,
            CodigoPais = pais.Codigo,
            Pais = pais,
            FusoHorarioPadrao = fusoHorario,
            JornadaContratualId = jornada.Id,
            JornadaContratual = jornada,
            GestorId = gestor?.Id,
            Gestor = gestor,
            Ativo = true,
            DeveAlterarSenha = true,
            CriadoEmUtc = relogio.GetUtcNow().UtcDateTime
        };
        funcionario.HashSenha = hasherSenha.HashPassword(funcionario, request.SenhaInicial);

        db.Funcionarios.Add(funcionario);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_Funcionarios_Email"
            })
        {
            throw new ConflitoException("Já existe um funcionário cadastrado com este e-mail.");
        }

        return FuncionarioResponse.De(funcionario);
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
