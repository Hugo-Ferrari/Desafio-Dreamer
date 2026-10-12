using backend.Data;
using backend.Domain;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using backend.Models.Enums;
using backend.Security;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AjusteService(PontoDbContext db,FabricaMarcacao fabricaMarcacao,FechamentoPeriodoService fechamentos,TimeProvider relogio)
{
    public async Task<SolicitacaoAjusteResponse> CriarAsync(
        Guid funcionarioId,
        CriarSolicitacaoAjusteRequest request,
        CancellationToken ct)
    {
        ValidarCampos(request, AgoraUtc);

        var funcionario = await db.Funcionarios.FirstOrDefaultAsync(f => f.Id == funcionarioId, ct)
            ?? throw new RecursoNaoEncontradoException("Funcionário não encontrado.");

        var marcacaoAlvo = await BuscarMarcacaoAlvoAsync(funcionarioId, request.MarcacaoAlvoId, ct);
        var fusoProposto = request.FusoHorarioProposto ?? marcacaoAlvo?.FusoHorarioId ?? funcionario.FusoHorarioPadrao;
        var fuso = FusoHorario.Obter(fusoProposto);

        var dataAfetada = request.OcorridoEmProposto is { } ocorridoEm
            ? FusoHorario.DataLocal(fuso, ocorridoEm.UtcDateTime)
            : marcacaoAlvo!.DataJornadaLocal;

        await fechamentos.GarantirAbertoAsync(funcionario.CodigoPais, dataAfetada, ct);

        var solicitacao = new SolicitacaoAjuste
        {
            FuncionarioId = funcionarioId,
            Funcionario = funcionario,
            TipoSolicitacao = request.TipoSolicitacao,
            MarcacaoAlvoId = marcacaoAlvo?.Id,
            TipoMarcacaoProposto = request.TipoMarcacaoProposto,
            OcorridoEmUtcProposto = request.OcorridoEmProposto?.UtcDateTime,
            FusoHorarioProposto = request.TipoSolicitacao == TipoSolicitacaoAjuste.Anular ? null : fusoProposto,
            Motivo = request.Motivo.Trim(),
            CriadaEmUtc = AgoraUtc
        };

        db.SolicitacoesAjuste.Add(solicitacao);
        await db.SaveChangesAsync(ct);

        return SolicitacaoAjusteResponse.De(solicitacao);
    }

    public async Task<IReadOnlyList<SolicitacaoAjusteResponse>> ListarAsync(
        UsuarioAtual usuario,
        StatusSolicitacaoAjuste? status,
        CancellationToken ct)
    {
        var funcionariosVisiveis = db.Funcionarios.VisiveisPara(usuario).Select(f => f.Id);

        var consulta = db.SolicitacoesAjuste
            .AsNoTracking()
            .Include(s => s.Funcionario)
            .Where(s => funcionariosVisiveis.Contains(s.FuncionarioId));

        if (status is { } statusFiltro)
        {
            consulta = consulta.Where(s => s.Status == statusFiltro);
        }

        var solicitacoes = await consulta.OrderByDescending(s => s.CriadaEmUtc).ToListAsync(ct);
        return solicitacoes.Select(SolicitacaoAjusteResponse.De).ToList();
    }

    public async Task<SolicitacaoAjusteResponse> AprovarAsync(
        UsuarioAtual avaliador,
        Guid solicitacaoId,
        string? comentario,
        CancellationToken ct)
    {
        var solicitacao = await BuscarParaAvaliacaoAsync(avaliador, solicitacaoId, ct);

        solicitacao.Aprovar(avaliador.Id, AgoraUtc, comentario);
        await AplicarAsync(solicitacao, ct);
        await db.SaveChangesAsync(ct);

        return SolicitacaoAjusteResponse.De(solicitacao);
    }

    public async Task<SolicitacaoAjusteResponse> RejeitarAsync(
        UsuarioAtual avaliador,
        Guid solicitacaoId,
        string? comentario,
        CancellationToken ct)
    {
        var solicitacao = await BuscarParaAvaliacaoAsync(avaliador, solicitacaoId, ct);

        solicitacao.Rejeitar(avaliador.Id, AgoraUtc, comentario);
        await db.SaveChangesAsync(ct);

        return SolicitacaoAjusteResponse.De(solicitacao);
    }

    private async Task AplicarAsync(SolicitacaoAjuste solicitacao, CancellationToken ct)
    {
        var funcionario = solicitacao.Funcionario;
        var marcacaoAlvo = solicitacao.MarcacaoAlvo;

        if (marcacaoAlvo is not null)
        {
            await fechamentos.GarantirAbertoAsync(funcionario.CodigoPais, marcacaoAlvo.DataJornadaLocal, ct);
            marcacaoAlvo.Anular();
        }

        if (solicitacao.TipoSolicitacao == TipoSolicitacaoAjuste.Anular)
        {
            await ValidarSequenciaResultanteAsync(funcionario.Id, marcacaoAlvo!.Id, null, ct);
            return;
        }

        var ocorridoEmUtc = solicitacao.OcorridoEmUtcProposto!.Value;
        var modalidade = marcacaoAlvo?.ModalidadeTrabalho
            ?? await BuscarModalidadeDeReferenciaAsync(funcionario.Id, ocorridoEmUtc, ct);

        var novaMarcacao = await fabricaMarcacao.CriarAsync(new DadosNovaMarcacao(
            funcionario.Id,
            funcionario.CodigoPais,
            solicitacao.TipoMarcacaoProposto ?? marcacaoAlvo!.Tipo,
            ocorridoEmUtc,
            solicitacao.FusoHorarioProposto ?? funcionario.FusoHorarioPadrao,
            modalidade,
            OrigemRegistro.Ajuste,
            SubstituiMarcacaoId: marcacaoAlvo?.Id), ct);

        await ValidarSequenciaResultanteAsync(funcionario.Id, marcacaoAlvo?.Id, novaMarcacao, ct);
    }

    private async Task ValidarSequenciaResultanteAsync(
        Guid funcionarioId,
        Guid? marcacaoSubstituidaId,
        MarcacaoPonto? novaMarcacao,
        CancellationToken ct)
    {
        var marcacoes = await db.MarcacoesPonto
            .AsNoTracking()
            .Where(marcacao => marcacao.FuncionarioId == funcionarioId && !marcacao.Anulada)
            .ToListAsync(ct);

        if (marcacaoSubstituidaId is { } id)
        {
            marcacoes.RemoveAll(marcacao => marcacao.Id == id);
        }

        if (novaMarcacao is not null)
        {
            marcacoes.Add(novaMarcacao);
        }

        MarcacaoPonto? anterior = null;
        foreach (var marcacao in marcacoes.OrderBy(marcacao => marcacao.OcorridoEmUtc))
        {
            if (anterior is not null && marcacao.OcorridoEmUtc <= anterior.OcorridoEmUtc)
            {
                throw new RegraNegocioException("O ajuste deixaria duas marcações no mesmo instante.");
            }

            var permitidas = SequenciaMarcacao.ProximosTiposPermitidos(anterior, marcacao.OcorridoEmUtc);
            if (!permitidas.Contains(marcacao.Tipo))
            {
                throw new RegraNegocioException(
                    $"O ajuste deixaria a sequência de marcações inválida. Próximas permitidas: {string.Join(", ", permitidas)}.");
            }

            anterior = marcacao;
        }
    }

    private async Task<SolicitacaoAjuste> BuscarParaAvaliacaoAsync(
        UsuarioAtual avaliador,
        Guid solicitacaoId,
        CancellationToken ct)
    {
        var solicitacao = await db.SolicitacoesAjuste
            .Include(s => s.Funcionario)
            .Include(s => s.MarcacaoAlvo)
            .FirstOrDefaultAsync(s => s.Id == solicitacaoId, ct)
            ?? throw new RecursoNaoEncontradoException("Solicitação de ajuste não encontrada.");

        GarantirPodeAvaliar(avaliador, solicitacao);
        return solicitacao;
    }

    private async Task<MarcacaoPonto?> BuscarMarcacaoAlvoAsync(Guid funcionarioId, Guid? marcacaoAlvoId, CancellationToken ct)
    {
        if (marcacaoAlvoId is null)
        {
            return null;
        }

        var marcacao = await db.MarcacoesPonto.AsNoTracking().FirstOrDefaultAsync(m => m.Id == marcacaoAlvoId && m.FuncionarioId == funcionarioId, ct)
            ?? throw new RecursoNaoEncontradoException("Marcação alvo não encontrada.");

        if (marcacao.Anulada)
        {
            throw new RegraNegocioException("A marcação alvo já está anulada.");
        }

        return marcacao;
    }

    private async Task<ModalidadeTrabalho> BuscarModalidadeDeReferenciaAsync(
        Guid funcionarioId,
        DateTime ocorridoEmUtc,
        CancellationToken ct)
    {
        var modalidadeAnterior = await db.MarcacoesPonto
            .Where(m => m.FuncionarioId == funcionarioId && !m.Anulada && m.OcorridoEmUtc <= ocorridoEmUtc)
            .OrderByDescending(m => m.OcorridoEmUtc)
            .Select(m => (ModalidadeTrabalho?)m.ModalidadeTrabalho)
            .FirstOrDefaultAsync(ct);

        return modalidadeAnterior ?? ModalidadeTrabalho.Escritorio;
    }

    private static void GarantirPodeAvaliar(UsuarioAtual avaliador, SolicitacaoAjuste solicitacao)
    {
        if (solicitacao.FuncionarioId == avaliador.Id)
        {
            throw new AcessoNegadoException("Não é permitido avaliar a própria solicitação.");
        }

        var autorizado = avaliador.Perfil == PerfilFuncionario.RH
            || (avaliador.Perfil == PerfilFuncionario.Gestor && solicitacao.Funcionario.GestorId == avaliador.Id);

        if (!autorizado)
        {
            throw new AcessoNegadoException("Apenas o gestor do funcionário ou o RH podem avaliar esta solicitação.");
        }
    }

    private static void ValidarCampos(CriarSolicitacaoAjusteRequest request, DateTime agoraUtc)
    {
        var erro = request.TipoSolicitacao switch
        {
            TipoSolicitacaoAjuste.Adicionar when request.MarcacaoAlvoId is not null =>
                "Solicitações de inclusão não devem informar MarcacaoAlvoId.",
            TipoSolicitacaoAjuste.Adicionar when request.TipoMarcacaoProposto is null || request.OcorridoEmProposto is null =>
                "Para adicionar uma marcação informe TipoMarcacaoProposto e OcorridoEmProposto.",
            TipoSolicitacaoAjuste.Alterar when request.MarcacaoAlvoId is null || request.OcorridoEmProposto is null =>
                "Para alterar uma marcação informe MarcacaoAlvoId e OcorridoEmProposto.",
            TipoSolicitacaoAjuste.Anular when request.MarcacaoAlvoId is null =>
                "Para anular uma marcação informe MarcacaoAlvoId.",
            _ when request.OcorridoEmProposto?.UtcDateTime > agoraUtc =>
                "O horário proposto não pode estar no futuro.",
            _ => null
        };

        if (erro is not null)
        {
            throw new RegraNegocioException(erro);
        }
    }

    private DateTime AgoraUtc => relogio.GetUtcNow().UtcDateTime;
}
