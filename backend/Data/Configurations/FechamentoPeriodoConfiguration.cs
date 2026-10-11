using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class FechamentoPeriodoConfiguration : IEntityTypeConfiguration<FechamentoPeriodo>
{
    public void Configure(EntityTypeBuilder<FechamentoPeriodo> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_FechamentosPeriodo_Ano", "\"Ano\" > 0");
            table.HasCheckConstraint("CK_FechamentosPeriodo_Mes", "\"Mes\" BETWEEN 1 AND 12");
        });

        builder.HasKey(fechamento => fechamento.Id);

        builder.Property(fechamento => fechamento.CodigoPais).HasMaxLength(2);
        builder.Property(fechamento => fechamento.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(fechamento => new { fechamento.CodigoPais, fechamento.Ano, fechamento.Mes }).IsUnique();

        builder.HasOne(fechamento => fechamento.Pais)
            .WithMany(pais => pais.Fechamentos)
            .HasForeignKey(fechamento => fechamento.CodigoPais)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(fechamento => fechamento.FechadoPor)
            .WithMany()
            .HasForeignKey(fechamento => fechamento.FechadoPorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
