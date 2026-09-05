using Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Connect;

public static class AdicionarSqlServer
{
    public static IServiceCollection AplicarSqlServer(this IServiceCollection services,
                                                      IConfiguration configuration)
    {
        var stringConnection = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "A connection string 'DefaultConnection' não foi configurada.");

        services.AddDbContext<MeuContext>(options => options.UseSqlServer(stringConnection));

        return services;
    }
}
