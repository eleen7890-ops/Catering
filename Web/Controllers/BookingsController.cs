using System.Security.Claims;
using Application.Bookings;
using Application.Menus;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

public sealed class BookingsController(
    BookingService bookingService,
    BookingHistoryService historyService,
    MenuRecommendationService recommendations) : Controller
{
    [HttpGet]
    public IActionResult Create() => View(new BookingForm
    {
        EventDate = DateTime.Today.AddDays(7).AddHours(18),
        CustomerName = User.Identity?.IsAuthenticated == true ? User.Identity.Name ?? "" : ""
    });

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Create(BookingForm form, CancellationToken cancellationToken)
    {
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

        var preferences = new MenuPreferences(form.EventType, form.Cuisine, form.DietaryPreference,
            form.Taste, form.Mood, form.GuestCount, form.BudgetPerGuest, form.Currency);
        var eventDate = new DateTimeOffset(
            DateTime.SpecifyKind(form.EventDate, DateTimeKind.Unspecified),
            TimeSpan.FromMinutes(-form.TimezoneOffsetMinutes));
        var request = new BookingRequest(preferences, form.CustomerName, form.CustomerEmail,
            form.CustomerPhone, eventDate, form.FulfillmentMethod, form.DeliveryAddress,
            form.CakeDescription, cakeImage);

        try
        {
            var menu = await recommendations.RecommendAsync(preferences, cancellationToken);
            if (menu is null)
            {
                ModelState.AddModelError(string.Empty, "No complete menu fits those preferences and budget.");
                return View(form);
            }

            var receipt = await bookingService.CreateAsync(request,
                User.FindFirstValue(ClaimTypes.NameIdentifier), cancellationToken);
            if (receipt is null)
            {
                ModelState.AddModelError(string.Empty, "No complete menu fits those preferences and budget.");
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
