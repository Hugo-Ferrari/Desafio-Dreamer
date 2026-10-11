using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Data.Configurations;

public class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
{
    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.HasKey(funcionario => funcionario.Id);

        builder.Property(funcionario => funcionario.Nome).HasMaxLength(150);
        builder.Property(funcionario => funcionario.Email).HasMaxLength(254);
        builder.Property(funcionario => funcionario.HashSenha).HasMaxLength(255);
        builder.Property(funcionario => funcionario.Perfil).HasConversion<string>().HasMaxLength(20);
        builder.Property(funcionario => funcionario.CodigoPais).HasMaxLength(2);
        builder.Property(funcionario => funcionario.FusoHorarioPadrao).HasMaxLength(100);

        builder.HasIndex(funcionario => funcionario.Email).IsUnique();

        builder.HasOne(funcionario => funcionario.Pais)
            .WithMany(pais => pais.Funcionarios)
            .HasForeignKey(funcionario => funcionario.CodigoPais)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(funcionario => funcionario.JornadaContratual)
            .WithMany(jornada => jornada.Funcionarios)
            .HasForeignKey(funcionario => funcionario.JornadaContratualId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(funcionario => funcionario.Gestor)
            .WithMany(gestor => gestor.Subordinados)
            .HasForeignKey(funcionario => funcionario.GestorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
