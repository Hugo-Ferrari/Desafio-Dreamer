using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class FeriadoConfiguration : IEntityTypeConfiguration<Feriado>
{
    public void Configure(EntityTypeBuilder<Feriado> builder)
    {
        builder.HasKey(feriado => feriado.Id);

        builder.Property(feriado => feriado.CodigoPais).HasMaxLength(2);
        builder.Property(feriado => feriado.Nome).HasMaxLength(150);

        builder.HasIndex(feriado => new { feriado.CodigoPais, feriado.Data }).IsUnique();

        builder.HasOne(feriado => feriado.Pais)
            .WithMany(pais => pais.Feriados)
            .HasForeignKey(feriado => feriado.CodigoPais)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
