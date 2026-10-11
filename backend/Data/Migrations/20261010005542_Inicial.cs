using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JornadasContratuais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MinutosDiarios = table.Column<int>(type: "integer", nullable: false),
                    MinutosSemanais = table.Column<int>(type: "integer", nullable: false),
                    MinutosIntervaloMinimo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JornadasContratuais", x => x.Id);
                    table.CheckConstraint("CK_JornadasContratuais_MinutosDiarios", "\"MinutosDiarios\" > 0");
                    table.CheckConstraint("CK_JornadasContratuais_MinutosIntervaloMinimo", "\"MinutosIntervaloMinimo\" >= 0");
                    table.CheckConstraint("CK_JornadasContratuais_MinutosSemanais", "\"MinutosSemanais\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "Paises",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FusoHorarioPadrao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paises", x => x.Codigo);
                });

            migrationBuilder.CreateTable(
                name: "Feriados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoPais = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feriados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feriados_Paises_CodigoPais",
                        column: x => x.CodigoPais,
                        principalTable: "Paises",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Funcionarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    HashSenha = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Perfil = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CodigoPais = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    FusoHorarioPadrao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    JornadaContratualId = table.Column<Guid>(type: "uuid", nullable: false),
                    GestorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEmUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funcionarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Funcionarios_Funcionarios_GestorId",
                        column: x => x.GestorId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Funcionarios_JornadasContratuais_JornadaContratualId",
                        column: x => x.JornadaContratualId,
                        principalTable: "JornadasContratuais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Funcionarios_Paises_CodigoPais",
                        column: x => x.CodigoPais,
                        principalTable: "Paises",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ausencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FuncionarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    DataFim = table.Column<DateOnly>(type: "date", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ausencias", x => x.Id);
                    table.CheckConstraint("CK_Ausencias_Periodo", "\"DataFim\" >= \"DataInicio\"");
                    table.ForeignKey(
                        name: "FK_Ausencias_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FechamentosPeriodo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoPais = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FechadoPorId = table.Column<Guid>(type: "uuid", nullable: true),
                    FechadoEmUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FechamentosPeriodo", x => x.Id);
                    table.CheckConstraint("CK_FechamentosPeriodo_Ano", "\"Ano\" > 0");
                    table.CheckConstraint("CK_FechamentosPeriodo_Mes", "\"Mes\" BETWEEN 1 AND 12");
                    table.ForeignKey(
                        name: "FK_FechamentosPeriodo_Funcionarios_FechadoPorId",
                        column: x => x.FechadoPorId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FechamentosPeriodo_Paises_CodigoPais",
                        column: x => x.CodigoPais,
                        principalTable: "Paises",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarcacoesPonto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FuncionarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OcorridoEmUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RegistradoEmUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FusoHorarioId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeslocamentoUtcMinutos = table.Column<int>(type: "integer", nullable: false),
                    DataJornadaLocal = table.Column<DateOnly>(type: "date", nullable: false),
                    ModalidadeTrabalho = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OrigemRegistro = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Anulada = table.Column<bool>(type: "boolean", nullable: false),
                    SubstituiMarcacaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChaveIdempotencia = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarcacoesPonto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarcacoesPonto_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarcacoesPonto_MarcacoesPonto_SubstituiMarcacaoId",
                        column: x => x.SubstituiMarcacaoId,
                        principalTable: "MarcacoesPonto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolicitacoesAjuste",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FuncionarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoSolicitacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MarcacaoAlvoId = table.Column<Guid>(type: "uuid", nullable: true),
                    TipoMarcacaoProposto = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    OcorridoEmUtcProposto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FusoHorarioProposto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Motivo = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AvaliadorId = table.Column<Guid>(type: "uuid", nullable: true),
                    AvaliadaEmUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ComentarioAvaliacao = table.Column<string>(type: "text", nullable: true),
                    CriadaEmUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitacoesAjuste", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitacoesAjuste_Funcionarios_AvaliadorId",
                        column: x => x.AvaliadorId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitacoesAjuste_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitacoesAjuste_MarcacoesPonto_MarcacaoAlvoId",
                        column: x => x.MarcacaoAlvoId,
                        principalTable: "MarcacoesPonto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ausencias_FuncionarioId",
                table: "Ausencias",
                column: "FuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_FechamentosPeriodo_CodigoPais_Ano_Mes",
                table: "FechamentosPeriodo",
                columns: new[] { "CodigoPais", "Ano", "Mes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FechamentosPeriodo_FechadoPorId",
                table: "FechamentosPeriodo",
                column: "FechadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Feriados_CodigoPais_Data",
                table: "Feriados",
                columns: new[] { "CodigoPais", "Data" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_CodigoPais",
                table: "Funcionarios",
                column: "CodigoPais");

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_Email",
                table: "Funcionarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_GestorId",
                table: "Funcionarios",
                column: "GestorId");

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_JornadaContratualId",
                table: "Funcionarios",
                column: "JornadaContratualId");

            migrationBuilder.CreateIndex(
                name: "IX_MarcacoesPonto_ChaveIdempotencia",
                table: "MarcacoesPonto",
                column: "ChaveIdempotencia",
                unique: true,
                filter: "\"ChaveIdempotencia\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MarcacoesPonto_FuncionarioId_DataJornadaLocal",
                table: "MarcacoesPonto",
                columns: new[] { "FuncionarioId", "DataJornadaLocal" });

            migrationBuilder.CreateIndex(
                name: "IX_MarcacoesPonto_FuncionarioId_OcorridoEmUtc",
                table: "MarcacoesPonto",
                columns: new[] { "FuncionarioId", "OcorridoEmUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MarcacoesPonto_SubstituiMarcacaoId",
                table: "MarcacoesPonto",
                column: "SubstituiMarcacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesAjuste_AvaliadorId",
                table: "SolicitacoesAjuste",
                column: "AvaliadorId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesAjuste_FuncionarioId",
                table: "SolicitacoesAjuste",
                column: "FuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesAjuste_MarcacaoAlvoId",
                table: "SolicitacoesAjuste",
                column: "MarcacaoAlvoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ausencias");

            migrationBuilder.DropTable(
                name: "FechamentosPeriodo");

            migrationBuilder.DropTable(
                name: "Feriados");

            migrationBuilder.DropTable(
                name: "SolicitacoesAjuste");

            migrationBuilder.DropTable(
                name: "MarcacoesPonto");

            migrationBuilder.DropTable(
                name: "Funcionarios");

            migrationBuilder.DropTable(
                name: "JornadasContratuais");

            migrationBuilder.DropTable(
                name: "Paises");
        }
    }
}
