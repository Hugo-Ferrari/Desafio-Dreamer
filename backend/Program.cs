using backend.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AdicionarApi(builder.Configuration)
    .AdicionarBancoDeDados(builder.Configuration)
    .AdicionarAutenticacaoJwt(builder.Configuration)
    .AdicionarServicosDeAplicacao();

var app = builder.Build();

await app.PrepararBancoDeDadosAsync();

app.UseExceptionHandler();
app.UseCors(ServiceCollectionExtensions.PoliticaCorsFrontend);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.Run();
