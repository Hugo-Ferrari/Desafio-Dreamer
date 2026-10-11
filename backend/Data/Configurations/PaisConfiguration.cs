using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class PaisConfiguration : IEntityTypeConfiguration<Pais>
{
    public void Configure(EntityTypeBuilder<Pais> builder)
    {
        builder.HasKey(pais => pais.Codigo);

        builder.Property(pais => pais.Codigo).HasMaxLength(2);
        builder.Property(pais => pais.Nome).HasMaxLength(100);
        builder.Property(pais => pais.FusoHorarioPadrao).HasMaxLength(100);
    }
}
