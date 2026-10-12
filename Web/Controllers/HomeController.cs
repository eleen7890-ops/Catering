using System.Diagnostics;
using System.Data.Common;
using Application.Abstractions;
using Application.Menus;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

public class HomeController(CuratedMenuService menus, ILogger<HomeController> logger) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        IReadOnlyList<CuratedMenuView> featured;
        var catalogUnavailable = false;
        try
        {
            featured = await menus.ListPublishedAsync(new CuratedMenuQuery(), 3, cancellationToken);
        }
        catch (Exception exception) when (exception.GetBaseException() is DbException)
        {
            logger.LogError(exception, "The curated menu catalog is unavailable.");
            featured = [];
            catalogUnavailable = true;
        }
        return View(new HomePageViewModel
        {
            Occasions = HomePageSamples.Occasions,
            CatalogUnavailable = catalogUnavailable,
            Packages = featured.Select(menu => new FeaturedPackage(
                menu.Id, menu.Name, menu.Description, menu.ImagePath, menu.Name,
                menu.Cuisine.ToString(), menu.PriceJod, menu.MinimumGuests, menu.MaximumGuests,
                menu.Dishes.Select(dish => dish.Name).ToArray())).ToArray()
        });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
