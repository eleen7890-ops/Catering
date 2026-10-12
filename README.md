# Gather catering

Gather is an ASP.NET Core MVC website backed by EF Core and SQL Server.

## Layers

- **Domain** holds plain menu, dish, booking, and account-related data types.
- **Application** owns curated menu listing, filtering, publishing, validation, and booking rules. Repository interfaces live here.
- **Infrastructure** implements repositories with EF Core and maps SQL Server tables.
- **Web** contains controllers, Razor views, view models, CSS, and JavaScript. Controllers call Application services; they do not query the DbContext.

Menus are complete, manually curated offerings. The site does not assemble a menu from individual dishes. A menu has one JOD price for its stated guest range and an ordered list of included dishes. Existing booking snapshots preserve the menu name, dish names, and amount charged when an order is placed.

## Apply migrations

Set `CATERING_CONNECTION_STRING` to a SQL Server connection string accessible from your machine. The design-time factory otherwise targets server `DESKTOP-85J3V31` and database `CateringDevelopment` with Windows authentication and `TrustServerCertificate=True`. The Web app also reads `ConnectionStrings:Catering` from its settings.
If you use the `sa` account, put `User Id=sa;Password=...` in the environment variable instead of committing the password to a settings file.

```powershell
$env:CATERING_CONNECTION_STRING = 'Server=DESKTOP-85J3V31;Database=CateringDevelopment;Trusted_Connection=True;TrustServerCertificate=True'
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project Infrastructure/Infrastructure.csproj --context CateringDbContext
dotnet run --project Web/Web.csproj
```

The new migrations are `AddCuratedMenus` and `AddBookingCuratedMenuSnapshot`. An idempotent SQL script is at `Infrastructure/Persistence/Scripts/CuratedMenus.sql`. No curated menus are seeded: the homepage shows an empty state until an admin creates and publishes one.

## Make an admin account

1. Apply migrations and register your own account at `/Account/Register`.
2. Set `GATHER_ADMIN_EMAIL` to that account's exact email address in the server environment.
3. Restart the Web app. Startup creates the Admin role if needed and assigns it to the existing account.
4. Sign out and sign in again, then visit `/admin/menus`.

There is no default admin password. Keep `GATHER_ADMIN_EMAIL` under server control. Menu images are referenced by paths such as `/images/japanese-night.jpg`; put those files in `Web/wwwroot/images` before publishing.

## Verify features

- **Homepage:** publish a menu in Admin, then reload `/`; its name, image, price, capacity, and included dishes should appear. Unpublished menus should not appear.
- **Browsing:** visit `/Menus` and combine cuisine, vibe, and event filters. With no matches, the page should show an empty state.
- **Details:** open a card and check the full dish list and serving quantities. A draft or deleted menu URL should return 404.
- **Admin:** an anonymous visitor should be redirected to sign in; a signed-in non-admin should be denied. As Admin, create, edit, add/remove dishes, publish/unpublish, and delete a menu. Invalid prices, guest ranges, paths, and empty dish lists should be rejected.
- **Booking:** select a published menu at `/Bookings/Create`. The price comes from the database, and the confirmation lists that menu's dishes.

The older `MenuItems` table remains for compatibility with existing database data. The automatic dish-combination service is no longer used by the website.
