using Application.Bookings;
using Application.Menus;

namespace Web.Models;

public sealed record BookingConfirmation(BookingReceipt Receipt, CuratedMenuView Menu);
