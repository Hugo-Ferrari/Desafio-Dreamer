using backend.Models;
using backend.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class SemeadorDados(PontoDbContext db, IPasswordHasher<Funcionario> hasherSenha, TimeProvider relogio)
{
    public const string SenhaPadrao = "Senha@123";

    public async Task SemearAsync(CancellationToken ct = default)
    {
        if (await db.Paises.AnyAsync(ct))
        {
            return;
        }

        var brasil = new Pais { Codigo = "BR", Nome = "Brasil", FusoHorarioPadrao = "America/Sao_Paulo" };
        var portugal = new Pais { Codigo = "PT", Nome = "Portugal", FusoHorarioPadrao = "Europe/Lisbon" };

        var jornadaBrasil = new JornadaContratual
        {
            Nome = "CLT 44h", MinutosDiarios = 528, MinutosSemanais = 2640, MinutosIntervaloMinimo = 60
        };
        var jornadaPortugal = new JornadaContratual
        {
            Nome = "Portugal 40h", MinutosDiarios = 480, MinutosSemanais = 2400, MinutosIntervaloMinimo = 60
        };

        var rh = CriarFuncionario("Ana Ribeiro", "rh@dreams.local", PerfilFuncionario.RH, brasil, jornadaBrasil);
        var gestor = CriarFuncionario("Bruno Gestor", "gestor@dreams.local", PerfilFuncionario.Gestor, brasil, jornadaBrasil);
        var colaboradorBrasil = CriarFuncionario(
            "Carla Souza", "colaborador@dreams.local", PerfilFuncionario.Colaborador, brasil, jornadaBrasil, gestor);
        var colaboradorPortugal = CriarFuncionario(
            "Diogo Almeida", "colaborador.pt@dreams.local", PerfilFuncionario.Colaborador, portugal, jornadaPortugal, gestor);

        db.Paises.AddRange(brasil, portugal);
        db.JornadasContratuais.AddRange(jornadaBrasil, jornadaPortugal);
        db.Funcionarios.AddRange(rh, gestor, colaboradorBrasil, colaboradorPortugal);
        db.Feriados.AddRange(FeriadosBrasil2026().Concat(FeriadosPortugal2026()));

        await db.SaveChangesAsync(ct);
    }

    private Funcionario CriarFuncionario(
        string nome,
        string email,
        PerfilFuncionario perfil,
        Pais pais,
        JornadaContratual jornada,
        Funcionario? gestor = null)
    {
        var funcionario = new Funcionario
        {
            Nome = nome,
            Email = email,
            Perfil = perfil,
            Pais = pais,
            FusoHorarioPadrao = pais.FusoHorarioPadrao,
            JornadaContratual = jornada,
            Gestor = gestor,
            CriadoEmUtc = relogio.GetUtcNow().UtcDateTime
        };

        funcionario.HashSenha = hasherSenha.HashPassword(funcionario, SenhaPadrao);
        return funcionario;
    }

    private static IEnumerable<Feriado> FeriadosBrasil2026() => CriarFeriados("BR",
    [
        (new DateOnly(2026, 1, 1), "Confraternização Universal"),
        (new DateOnly(2026, 4, 3), "Sexta-feira Santa"),
        (new DateOnly(2026, 4, 21), "Tiradentes"),
        (new DateOnly(2026, 5, 1), "Dia do Trabalho"),
        (new DateOnly(2026, 9, 7), "Independência do Brasil"),
        (new DateOnly(2026, 10, 12), "Nossa Senhora Aparecida"),
        (new DateOnly(2026, 11, 2), "Finados"),
        (new DateOnly(2026, 11, 15), "Proclamação da República"),
        (new DateOnly(2026, 11, 20), "Dia Nacional de Zumbi e da Consciência Negra"),
        (new DateOnly(2026, 12, 25), "Natal")
    ]);

    private static IEnumerable<Feriado> FeriadosPortugal2026() => CriarFeriados("PT",
    [
        (new DateOnly(2026, 1, 1), "Ano Novo"),
        (new DateOnly(2026, 4, 3), "Sexta-feira Santa"),
        (new DateOnly(2026, 4, 5), "Páscoa"),
        (new DateOnly(2026, 4, 25), "Dia da Liberdade"),
        (new DateOnly(2026, 5, 1), "Dia do Trabalhador"),
        (new DateOnly(2026, 6, 4), "Corpo de Deus"),
        (new DateOnly(2026, 6, 10), "Dia de Portugal"),
        (new DateOnly(2026, 8, 15), "Assunção de Nossa Senhora"),
        (new DateOnly(2026, 10, 5), "Implantação da República"),
        (new DateOnly(2026, 11, 1), "Dia de Todos os Santos"),
        (new DateOnly(2026, 12, 1), "Restauração da Independência"),
        (new DateOnly(2026, 12, 8), "Imaculada Conceição"),
        (new DateOnly(2026, 12, 25), "Natal")
    ]);

    private static IEnumerable<Feriado> CriarFeriados(string codigoPais, IEnumerable<(DateOnly Data, string Nome)> feriados) =>
        feriados.Select(feriado => new Feriado { CodigoPais = codigoPais, Data = feriado.Data, Nome = feriado.Nome });
}
