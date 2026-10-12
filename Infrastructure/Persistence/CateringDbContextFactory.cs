using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

public sealed class CateringDbContextFactory : IDesignTimeDbContextFactory<CateringDbContext>
{
    public CateringDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CATERING_CONNECTION_STRING")
            ?? @"Server=DESKTOP-85J3V31;Database=CateringDevelopment;Trusted_Connection=True;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<CateringDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new CateringDbContext(options);
    }
}
