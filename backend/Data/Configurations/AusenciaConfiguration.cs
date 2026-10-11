using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class AusenciaConfiguration : IEntityTypeConfiguration<Ausencia>
{
    public void Configure(EntityTypeBuilder<Ausencia> builder)
    {
        builder.ToTable(table =>
            table.HasCheckConstraint("CK_Ausencias_Periodo", "\"DataFim\" >= \"DataInicio\""));

        builder.HasKey(ausencia => ausencia.Id);

        builder.Property(ausencia => ausencia.Tipo).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(ausencia => ausencia.Funcionario)
            .WithMany(funcionario => funcionario.Ausencias)
            .HasForeignKey(ausencia => ausencia.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
