using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartQ.Application.Interfaces;
using SmartQ.Infrastructure.Persistence;
using SmartQ.Infrastructure.Services;

namespace SmartQ.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SmartQDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICounterService, CounterService>();
        services.AddScoped<IDisplayService, DisplayService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}
