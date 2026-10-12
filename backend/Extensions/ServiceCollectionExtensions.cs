using System.Text.Json.Serialization;
using backend.Data;
using backend.Exceptions;
using backend.Models;
using backend.Security;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace backend.Extensions;

public static class ServiceCollectionExtensions
{
    public const string PoliticaCorsFrontend = "Frontend";

    public static IServiceCollection AdicionarApi(this IServiceCollection services, IConfiguration configuracao)
    {
        services
            .AddControllers()
            .AddJsonOptions(opcoes =>
            {
                opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                opcoes.JsonSerializerOptions.RespectNullableAnnotations = true;
                opcoes.JsonSerializerOptions.RespectRequiredConstructorParameters = true;
            });

        services.AddProblemDetails();
        services.AddExceptionHandler<TratadorExcecoes>();
        services.AddOpenApi();

        var origensPermitidas = configuracao.GetSection("Cors:OrigensPermitidas").Get<string[]>() ?? [];
        services.AddCors(opcoes => opcoes.AddPolicy(PoliticaCorsFrontend, politica => politica
            .WithOrigins(origensPermitidas)
            .AllowAnyHeader()
            .AllowAnyMethod()));

        return services;
    }

    public static IServiceCollection AdicionarBancoDeDados(this IServiceCollection services, IConfiguration configuracao)
    {
        var connectionString = configuracao.GetConnectionString("Padrao")
            ?? throw new InvalidOperationException("A connection string 'Padrao' do PostgreSQL não foi configurada.");

        services.AddDbContext<PontoDbContext>(opcoes => opcoes.UseNpgsql(connectionString));
        services.AddScoped<SemeadorDados>();

        return services;
    }

    public static IServiceCollection AdicionarAutenticacaoJwt(this IServiceCollection services, IConfiguration configuracao)
    {
        services.AddOptions<ConfiguracaoJwt>()
            .Bind(configuracao.GetSection(ConfiguracaoJwt.Secao))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<ConfiguracaoJwt>>((bearer, opcoesJwt) =>
            {
                var jwt = opcoesJwt.Value;
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Emissor,
                    ValidAudience = jwt.Audiencia,
                    IssuerSigningKey = GeradorTokens.CriarChaveAssinatura(jwt.Chave),
                    NameClaimType = JwtRegisteredClaimNames.Name,
                    RoleClaimType = GeradorTokens.ClaimPerfil,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(
                GeradorTokens.PoliticaTrocaSenhaInicial,
                politica => politica
                    .RequireAuthenticatedUser()
                    .RequireClaim(GeradorTokens.ClaimTrocaSenhaInicial, "true"))
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireAssertion(context =>
                    context.User.FindFirst(GeradorTokens.ClaimTrocaSenhaInicial)?.Value != "true")
                .Build());

        services.AddSingleton<GeradorTokens>();
        services.AddSingleton<IPasswordHasher<Funcionario>, PasswordHasher<Funcionario>>();

        return services;
    }

    public static IServiceCollection AdicionarServicosDeAplicacao(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<AutenticacaoService>();
        services.AddScoped<FuncionarioService>();
        services.AddScoped<FechamentoPeriodoService>();
        services.AddScoped<FabricaMarcacao>();
        services.AddScoped<MarcacaoService>();
        services.AddScoped<EspelhoPontoService>();
        services.AddScoped<AjusteService>();
        services.AddScoped<EquipeService>();
        services.AddScoped<RelatorioService>();

        return services;
    }
}
