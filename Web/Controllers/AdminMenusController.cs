using Application.Menus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/menus")]
public sealed class AdminMenusController(CuratedMenuService menus) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await menus.ListAllForAdminAsync(cancellationToken));

    [HttpGet("create")]
    public IActionResult Create() => View("Edit", new MenuEditorForm());

    [HttpPost("create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuEditorForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View("Edit", form);
        try
        {
            var id = await menus.CreateAsync(form.ToCommand(), cancellationToken);
            TempData["MenuNotice"] = "Menu created.";
            return RedirectToAction(nameof(Edit), new { id });
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View("Edit", form);
        }
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var menu = await menus.GetForAdminAsync(id, cancellationToken);
        if (menu is null) return NotFound();
        ViewData["MenuId"] = id;
        return View(MenuEditorForm.FromMenu(menu));
    }

    [HttpPost("{id:guid}/edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MenuEditorForm form, CancellationToken cancellationToken)
    {
        ViewData["MenuId"] = id;
        if (!ModelState.IsValid) return View(form);
        try
        {
            if (!await menus.UpdateAsync(id, form.ToCommand(), cancellationToken)) return NotFound();
            TempData["MenuNotice"] = "Menu updated.";
            return RedirectToAction(nameof(Edit), new { id });
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(form);
        }
    }

    [HttpPost("{id:guid}/delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!await menus.DeleteAsync(id, cancellationToken)) return NotFound();
        TempData["MenuNotice"] = "Menu deleted.";
        return RedirectToAction(nameof(Index));
    }
}
