using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Extensions;

public static class WebApplicationExtensions
{
    public static async Task PrepararBancoDeDadosAsync(this WebApplication app)
    {
        await using var escopo = app.Services.CreateAsyncScope();

        var db = escopo.ServiceProvider.GetRequiredService<PontoDbContext>();
        await db.Database.MigrateAsync();

        if (app.Environment.IsDevelopment())
        {
            await escopo.ServiceProvider.GetRequiredService<SemeadorDados>().SemearAsync();
        }
    }
}
