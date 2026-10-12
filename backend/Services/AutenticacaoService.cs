using backend.Data;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using backend.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AutenticacaoService(
    PontoDbContext db,
    GeradorTokens geradorTokens,
    IPasswordHasher<Funcionario> hasherSenha,
    TimeProvider relogio)
{
    private const string MensagemCredenciaisInvalidas = "E-mail ou senha inválidos.";
    private const string MensagemTokenInvalido = "Token de renovação inválido ou expirado.";

    public async Task<TokensResponse> EntrarAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var funcionario = await db.Funcionarios.FirstOrDefaultAsync(f => f.Email == email && f.Ativo, ct);

        if (funcionario is null || !SenhaConfere(funcionario, request.Senha))
        {
            throw new CredenciaisInvalidasException(MensagemCredenciaisInvalidas);
        }

        var tokens = EmitirTokens(funcionario);
        await db.SaveChangesAsync(ct);
        return tokens;
    }

    public async Task<TokensResponse> RenovarAsync(string tokenRenovacao, CancellationToken ct)
    {
        var agora = AgoraUtc;
        var token = await BuscarTokenAsync(tokenRenovacao, ct);

        if (token is null || !token.EstaAtivo(agora) || !token.Funcionario.Ativo)
        {
            throw new CredenciaisInvalidasException(MensagemTokenInvalido);
        }

        token.Revogar(agora);
        var tokens = EmitirTokens(token.Funcionario);
        await db.SaveChangesAsync(ct);
        return tokens;
    }

    public async Task SairAsync(string tokenRenovacao, CancellationToken ct)
    {
        var token = await BuscarTokenAsync(tokenRenovacao, ct);
        if (token is null)
        {
            return;
        }

        token.Revogar(AgoraUtc);
        await db.SaveChangesAsync(ct);
    }

    public async Task AlterarSenhaInicialAsync(
        Guid funcionarioId,
        AlterarSenhaInicialRequest request,
        CancellationToken ct)
    {
        var funcionario = await db.Funcionarios.FirstOrDefaultAsync(f => f.Id == funcionarioId && f.Ativo, ct)
            ?? throw new RecursoNaoEncontradoException("Funcionário não encontrado.");

        if (!funcionario.DeveAlterarSenha)
        {
            throw new RegraNegocioException("Não há troca obrigatória de senha pendente para esta conta.");
        }

        if (hasherSenha.VerifyHashedPassword(funcionario, funcionario.HashSenha, request.NovaSenha) != PasswordVerificationResult.Failed)
        {
            throw new RegraNegocioException("A nova senha deve ser diferente da senha inicial.");
        }

        funcionario.HashSenha = hasherSenha.HashPassword(funcionario, request.NovaSenha);
        funcionario.DeveAlterarSenha = false;

        var agora = AgoraUtc;
        var tokensAtivos = await db.TokensRenovacao
            .Where(token => token.FuncionarioId == funcionarioId && token.RevogadoEmUtc == null)
            .ToListAsync(ct);
        foreach (var token in tokensAtivos)
        {
            token.Revogar(agora);
        }

        await db.SaveChangesAsync(ct);
    }

    private TokensResponse EmitirTokens(Funcionario funcionario)
    {
        var agora = AgoraUtc;
        var tokenAcesso = geradorTokens.GerarTokenAcesso(funcionario);
        var tokenRenovacao = GeradorTokens.GerarTokenRenovacao();

        db.TokensRenovacao.Add(new TokenRenovacao
        {
            FuncionarioId = funcionario.Id,
            TokenHash = GeradorTokens.CalcularHash(tokenRenovacao),
            CriadoEmUtc = agora,
            ExpiraEmUtc = agora + geradorTokens.ValidadeRenovacao
        });

        return new TokensResponse(tokenAcesso.Token, tokenAcesso.ExpiraEmUtc, tokenRenovacao);
    }

    private bool SenhaConfere(Funcionario funcionario, string senha) =>
        hasherSenha.VerifyHashedPassword(funcionario, funcionario.HashSenha, senha) != PasswordVerificationResult.Failed;

    private Task<TokenRenovacao?> BuscarTokenAsync(string tokenRenovacao, CancellationToken ct)
    {
        var hash = GeradorTokens.CalcularHash(tokenRenovacao);
        return db.TokensRenovacao
            .Include(token => token.Funcionario)
            .FirstOrDefaultAsync(token => token.TokenHash == hash, ct);
    }

    private DateTime AgoraUtc => relogio.GetUtcNow().UtcDateTime;
}
