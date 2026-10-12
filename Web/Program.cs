using System.Data.Common;
using Application.Bookings;
using Application.Menus;
using Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
if (builder.Environment.IsDevelopment())
    builder.Services.AddDataProtection().PersistKeysToFileSystem(
        new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")));

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration["CATERING_CONNECTION_STRING"]
    ?? builder.Configuration.GetConnectionString("Catering")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:Catering or CATERING_CONNECTION_STRING.");
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddScoped<CuratedMenuService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<BookingHistoryService>();
builder.Services.AddSingleton(TimeProvider.System);

var pricing = builder.Configuration.GetSection("Pricing");
var deliveryFees = pricing.GetSection("DeliveryFees").Get<Dictionary<string, decimal>>()
    ?? throw new InvalidOperationException("Configure Pricing:DeliveryFees.");
builder.Services.AddSingleton(new BookingPricingPolicy(
    pricing.GetValue<int>("CompletedBookingsForDiscount"),
    pricing.GetValue<decimal>("LoyaltyDiscountRate"), deliveryFees));

var app = builder.Build();

var adminEmail = Environment.GetEnvironmentVariable("GATHER_ADMIN_EMAIL");
if (!string.IsNullOrWhiteSpace(adminEmail))
{
    try
    {
        using var scope = app.Services.CreateScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        if (!await roles.RoleExistsAsync("Admin"))
        {
            var roleResult = await roles.CreateAsync(new IdentityRole("Admin"));
            if (!roleResult.Succeeded)
                throw new InvalidOperationException("Could not create the Admin role.");
        }
        var admin = await users.FindByEmailAsync(adminEmail.Trim());
        if (admin is not null && !await users.IsInRoleAsync(admin, "Admin"))
        {
            var assignment = await users.AddToRoleAsync(admin, "Admin");
            if (!assignment.Succeeded)
                throw new InvalidOperationException("Could not assign the Admin role.");
        }
        else if (admin is null)
            app.Logger.LogWarning("GATHER_ADMIN_EMAIL did not match an existing registered account.");
    }
    catch (Exception exception) when (exception.GetBaseException() is DbException)
    {
        app.Logger.LogError(exception, "Admin role setup will retry on the next startup when SQL Server is available.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
