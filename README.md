# Catering solution

This solution includes a catering booking website and its supporting layers.

## Current layers

- **Domain** contains plain catering entities and enums. It has no project or EF Core dependency.
- **Application** contains use cases, input and output records, and repository interfaces. It references Domain only.
- **Infrastructure** implements those repository interfaces with EF Core and stores customer accounts with ASP.NET Core Identity. It references Application and Domain.
- **Web** provides the MVC booking form, account pages, booking history, and birthday cake upload.

`MenuRecommendationService` asks `IMenuItemRepository` for available dishes, then chooses a main, side, and dessert that match the requested diet, cuisine, and food budget. Taste, mood, and event type rank matching combinations. It returns `null` if no complete menu fits.

`BookingService` validates contact and delivery details, recalculates the current menu, applies `BookingPricingPolicy`, saves a price snapshot through `IBookingRepository`, and returns a receipt. A signed-in customer's ID is supplied by the caller; `null` represents guest checkout. The caller must obtain that ID from trusted authentication state, never a form field.

Birthday bookings can include an optional cake description and reference photo. The MVC form shows these fields for birthdays and passes the photo as `CakeImageUpload` on `BookingRequest`. The service accepts JPEG, PNG, or WebP files up to 5 MB and stores the bytes and content type with the booking. Cake details are rejected for other event types.

`BookingHistoryService` lists a signed-in customer's bookings and saved menu items. Guest bookings are not automatically linked to accounts created later.

## Pricing choices

The requested budget is **per guest for food**. Delivery is added afterward. Loyalty discounts apply to the food subtotal and only to registered customers who already have the configured number of completed bookings. The threshold, rate, and delivery fees are constructor settings on `BookingPricingPolicy`; no business-specific amounts are hard-coded. Bookings store the prices and totals used when they were placed so later menu price changes do not rewrite history.

## Database setup

Infrastructure uses SQL Server. `CateringDbContext` maps the catalog, bookings, booking item snapshots, and Identity tables. The `InitialCreate` migration defines the schema. Its design-time factory reads `CATERING_CONNECTION_STRING`; if unset, it targets `DESKTOP-85J3V31` and the `CateringDevelopment` database with Windows authentication and `TrustServerCertificate=True`. No migration has been applied to a database yet.

The EF Core SQL Server provider, Identity EF stores, and EF design package are referenced by `Infrastructure.csproj`. The generated idempotent SQL is in `Infrastructure/Persistence/Scripts/InitialCreate.sql`. To apply the migration with EF tooling once a working connection is available:

```powershell
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project Infrastructure/Infrastructure.csproj --context CateringDbContext --connection '<SQL Server connection string>'
```

The fallback connection uses the Windows identity of the process running EF tools. It does not use the SQL Server `sa` account. To use `sa`, supply a full SQL authentication connection string through `CATERING_CONNECTION_STRING` and keep the password outside tracked files.

The MVC startup project will call `services.AddInfrastructure(connectionString)` and register the Application services and `BookingPricingPolicy`. It must obtain `customerId` from the authenticated user claim, never a form field.

## Run the website

Set `ConnectionStrings:Catering` in `Web/appsettings.json` or the `CATERING_CONNECTION_STRING` environment variable to a working SQL Server database. Adjust `Pricing` in `Web/appsettings.json` for the desired loyalty discount and delivery fees. Apply the EF migrations, then run:

```powershell
dotnet run --project Web/Web.csproj
```

The home page links to the booking form. Guests can submit requests; signed-in customers can also see their booking history. The booking confirmation shows the selected menu and price. A SQL Server database is required for registration, menu recommendations, and booking persistence. Add available menu items before making booking requests.
