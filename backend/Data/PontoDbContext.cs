using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class PontoDbContext(DbContextOptions<PontoDbContext> options) : DbContext(options)
{
    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<JornadaContratual> JornadasContratuais => Set<JornadaContratual>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<MarcacaoPonto> MarcacoesPonto => Set<MarcacaoPonto>();
    public DbSet<SolicitacaoAjuste> SolicitacoesAjuste => Set<SolicitacaoAjuste>();
    public DbSet<Feriado> Feriados => Set<Feriado>();
    public DbSet<Ausencia> Ausencias => Set<Ausencia>();
    public DbSet<FechamentoPeriodo> FechamentosPeriodo => Set<FechamentoPeriodo>();
    public DbSet<TokenRenovacao> TokensRenovacao => Set<TokenRenovacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PontoDbContext).Assembly);
    }
}
