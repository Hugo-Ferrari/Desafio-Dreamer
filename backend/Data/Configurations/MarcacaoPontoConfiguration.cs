using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class MarcacaoPontoConfiguration : IEntityTypeConfiguration<MarcacaoPonto>
{
    public void Configure(EntityTypeBuilder<MarcacaoPonto> builder)
    {
        builder.HasKey(marcacao => marcacao.Id);

        builder.Property(marcacao => marcacao.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(marcacao => marcacao.FusoHorarioId).HasMaxLength(100);
        builder.Property(marcacao => marcacao.ModalidadeTrabalho).HasConversion<string>().HasMaxLength(20);
        builder.Property(marcacao => marcacao.OrigemRegistro).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(marcacao => new { marcacao.FuncionarioId, marcacao.DataJornadaLocal });
        builder.HasIndex(marcacao => new { marcacao.FuncionarioId, marcacao.OcorridoEmUtc });
        builder.HasIndex(marcacao => marcacao.ChaveIdempotencia)
            .IsUnique()
            .HasFilter("\"ChaveIdempotencia\" IS NOT NULL");

        builder.HasOne(marcacao => marcacao.Funcionario)
            .WithMany(funcionario => funcionario.Marcacoes)
            .HasForeignKey(marcacao => marcacao.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(marcacao => marcacao.SubstituiMarcacao)
            .WithMany()
            .HasForeignKey(marcacao => marcacao.SubstituiMarcacaoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
