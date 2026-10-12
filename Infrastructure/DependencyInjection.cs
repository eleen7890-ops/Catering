using Application.Abstractions;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("A SQL Server connection string is required.", nameof(connectionString));

        services.AddDbContext<CateringDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<ICuratedMenuRepository, EfCuratedMenuRepository>();
        services.AddScoped<IBookingRepository, EfBookingRepository>();
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
            options.User.RequireUniqueEmail = true)
            .AddEntityFrameworkStores<CateringDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}
