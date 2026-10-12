using System.Data.Common;
using System.Security.Claims;
using Application.Bookings;
using Application.Abstractions;
using Application.Menus;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

public sealed class BookingsController(
    BookingService bookingService,
    BookingHistoryService historyService,
    CuratedMenuService menus,
    ILogger<BookingsController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(Guid? menuId, CancellationToken cancellationToken)
    {
        IReadOnlyList<CuratedMenuView> availableMenus;
        try
        {
            availableMenus = await menus.ListPublishedAsync(new CuratedMenuQuery(),
                cancellationToken: cancellationToken);
        }
        catch (Exception exception) when (exception.GetBaseException() is DbException)
        {
            logger.LogError(exception, "The curated menu catalog is unavailable.");
            availableMenus = [];
            ViewData["CatalogUnavailable"] = true;
        }
        ViewData["AvailableMenus"] = availableMenus;
        return View(new BookingForm
        {
            MenuId = menuId,
            GuestCount = availableMenus.FirstOrDefault(menu => menu.Id == menuId)?.MinimumGuests ?? 20,
            EventDate = DateTime.Today.AddDays(7).AddHours(18),
            CustomerName = User.Identity?.IsAuthenticated == true ? User.Identity.Name ?? "" : ""
        });
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Create(BookingForm form, CancellationToken cancellationToken)
    {
        var availableMenus = await menus.ListPublishedAsync(new CuratedMenuQuery(),
            cancellationToken: cancellationToken);
        ViewData["AvailableMenus"] = availableMenus;
        if (form.FulfillmentMethod == FulfillmentMethod.Delivery &&
            string.IsNullOrWhiteSpace(form.DeliveryAddress))
            ModelState.AddModelError(nameof(form.DeliveryAddress), "A delivery address is required.");
        if (form.EventType != EventType.Birthday &&
            (!string.IsNullOrWhiteSpace(form.CakeDescription) || form.CakeImage is not null))
            ModelState.AddModelError(nameof(form.CakeImage), "Cake details are only available for birthdays.");
        if (form.CakeImage is { Length: > 5 * 1024 * 1024 })
            ModelState.AddModelError(nameof(form.CakeImage), "The cake photo must be 5 MB or smaller.");
        if (!ModelState.IsValid) return View(form);

        CakeImageUpload? cakeImage = null;
        if (form.CakeImage is { Length: > 0 } upload)
        {
            await using var stream = new MemoryStream();
            await upload.CopyToAsync(stream, cancellationToken);
            cakeImage = new CakeImageUpload(upload.ContentType, stream.ToArray());
        }

        var eventDate = new DateTimeOffset(
            DateTime.SpecifyKind(form.EventDate, DateTimeKind.Unspecified),
            TimeSpan.FromMinutes(-form.TimezoneOffsetMinutes));
        var request = new BookingRequest(form.MenuId!.Value, form.EventType, form.DietaryPreference,
            form.Taste, form.GuestCount, form.CustomerName, form.CustomerEmail,
            form.CustomerPhone, eventDate, form.FulfillmentMethod, form.DeliveryAddress,
            form.CakeDescription, cakeImage);

        try
        {
            var menu = availableMenus.FirstOrDefault(item => item.Id == form.MenuId);
            if (menu is null)
            {
                ModelState.AddModelError(nameof(form.MenuId), "Choose an available menu.");
                return View(form);
            }

            var receipt = await bookingService.CreateAsync(request,
                User.FindFirstValue(ClaimTypes.NameIdentifier), cancellationToken);
            if (receipt is null)
            {
                ModelState.AddModelError(nameof(form.MenuId), "This menu is no longer available.");
                return View(form);
            }
            return View("Confirmation", new BookingConfirmation(receipt, menu));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(form);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(form);
        }
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> History(CancellationToken cancellationToken)
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return View(await historyService.ListAsync(customerId, cancellationToken));
    }
}
