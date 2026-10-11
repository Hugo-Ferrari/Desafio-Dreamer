using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class TokenRenovacaoConfiguration : IEntityTypeConfiguration<TokenRenovacao>
{
    public void Configure(EntityTypeBuilder<TokenRenovacao> builder)
    {
        builder.HasKey(token => token.Id);

        builder.Property(token => token.TokenHash).HasMaxLength(64);

        builder.HasIndex(token => token.TokenHash).IsUnique();

        builder.HasOne(token => token.Funcionario)
            .WithMany()
            .HasForeignKey(token => token.FuncionarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
