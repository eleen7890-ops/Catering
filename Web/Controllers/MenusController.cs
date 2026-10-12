using System.Data.Common;
using Application.Abstractions;
using Application.Menus;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

public sealed class MenusController(CuratedMenuService menus, ILogger<MenusController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(Cuisine? cuisine, Mood? vibe, EventType? eventType,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest("Choose valid menu filters.");
        IReadOnlyList<CuratedMenuView> results;
        var catalogUnavailable = false;
        try
        {
            results = await menus.ListPublishedAsync(
                new CuratedMenuQuery(cuisine, vibe, eventType), cancellationToken: cancellationToken);
        }
        catch (Exception exception) when (exception.GetBaseException() is DbException)
        {
            logger.LogError(exception, "The curated menu catalog is unavailable.");
            results = [];
            catalogUnavailable = true;
        }
        return View(new MenuListViewModel
        {
            Cuisine = cuisine,
            Vibe = vibe,
            Event = eventType,
            Menus = results,
            CatalogUnavailable = catalogUnavailable
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var menu = await menus.GetPublishedAsync(id, cancellationToken);
        return menu is null ? NotFound() : View(menu);
    }
}
