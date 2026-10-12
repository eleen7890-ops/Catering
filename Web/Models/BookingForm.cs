using System.ComponentModel.DataAnnotations;
using Domain;
using Microsoft.AspNetCore.Http;

namespace Web.Models;

public sealed class BookingForm
{
    public EventType EventType { get; set; } = EventType.Birthday;
    public Cuisine Cuisine { get; set; } = Cuisine.MiddleEastern;
    public DietaryPreference DietaryPreference { get; set; }
    public TastePreference Taste { get; set; }
    public Mood Mood { get; set; }

    [Range(1, 10000)]
    public int GuestCount { get; set; } = 20;

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal BudgetPerGuest { get; set; } = 20;

    [Required, StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "JOD";

    [Required, StringLength(150)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required, Phone, StringLength(40)]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    // JavaScript supplies the browser's UTC offset, in minutes.
    [Range(-840, 840)]
    public int TimezoneOffsetMinutes { get; set; }

    public FulfillmentMethod FulfillmentMethod { get; set; }

    [StringLength(500)]
    public string? DeliveryAddress { get; set; }

    [StringLength(1000)]
    public string? CakeDescription { get; set; }

    public IFormFile? CakeImage { get; set; }
}
