using Application.Bookings;
using Application.Menus;
using Infrastructure;
using Microsoft.AspNetCore.DataProtection;

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
builder.Services.AddScoped<MenuRecommendationService>();
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
