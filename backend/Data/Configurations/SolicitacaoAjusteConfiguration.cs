using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class SolicitacaoAjusteConfiguration : IEntityTypeConfiguration<SolicitacaoAjuste>
{
    public void Configure(EntityTypeBuilder<SolicitacaoAjuste> builder)
    {
        builder.HasKey(solicitacao => solicitacao.Id);

        builder.Property(solicitacao => solicitacao.TipoSolicitacao).HasConversion<string>().HasMaxLength(20);
        builder.Property(solicitacao => solicitacao.TipoMarcacaoProposto).HasConversion<string>().HasMaxLength(20);
        builder.Property(solicitacao => solicitacao.FusoHorarioProposto).HasMaxLength(100);
        builder.Property(solicitacao => solicitacao.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(solicitacao => solicitacao.Funcionario)
            .WithMany(funcionario => funcionario.SolicitacoesAjuste)
            .HasForeignKey(solicitacao => solicitacao.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(solicitacao => solicitacao.MarcacaoAlvo)
            .WithMany()
            .HasForeignKey(solicitacao => solicitacao.MarcacaoAlvoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(solicitacao => solicitacao.Avaliador)
            .WithMany()
            .HasForeignKey(solicitacao => solicitacao.AvaliadorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
