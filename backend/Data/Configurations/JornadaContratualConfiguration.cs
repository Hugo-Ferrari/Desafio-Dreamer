using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class JornadaContratualConfiguration : IEntityTypeConfiguration<JornadaContratual>
{
    public void Configure(EntityTypeBuilder<JornadaContratual> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_JornadasContratuais_MinutosDiarios", "\"MinutosDiarios\" > 0");
            table.HasCheckConstraint("CK_JornadasContratuais_MinutosSemanais", "\"MinutosSemanais\" > 0");
            table.HasCheckConstraint("CK_JornadasContratuais_MinutosIntervaloMinimo", "\"MinutosIntervaloMinimo\" >= 0");
        });

        builder.HasKey(jornada => jornada.Id);

        builder.Property(jornada => jornada.Nome).HasMaxLength(100);
    }
}
