using System.ComponentModel.DataAnnotations;
using Application.Menus;
using Domain;

namespace Web.Models;

public sealed class MenuEditorForm : IValidatableObject
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;
    [Required, StringLength(1500)]
    public string Description { get; set; } = string.Empty;
    [Required, StringLength(500), RegularExpression(@"^/images/(?!.*\.\.)[A-Za-z0-9/_\-.]+$",
        ErrorMessage = "Use a local path such as /images/japanese-night.jpg.")]
    public string ImagePath { get; set; } = string.Empty;
    [Range(0.01, 1000000)]
    public decimal PriceJod { get; set; }
    [Range(1, 100000)]
    public int MinimumGuests { get; set; } = 8;
    [Range(1, 100000)]
    public int MaximumGuests { get; set; } = 12;
    [EnumDataType(typeof(Cuisine))]
    public Cuisine Cuisine { get; set; }
    [EnumDataType(typeof(Mood))]
    public Mood Vibe { get; set; }
    [EnumDataType(typeof(EventType))]
    public EventType Event { get; set; }
    public bool IsPublished { get; set; }
    public List<MenuDishForm> Dishes { get; set; } = [new()];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MaximumGuests < MinimumGuests)
            yield return new ValidationResult("Maximum guests must be at least minimum guests.",
                [nameof(MaximumGuests)]);
        if (Dishes is null || Dishes.Count == 0)
            yield return new ValidationResult("Add at least one dish.", [nameof(Dishes)]);
    }

    public SaveCuratedMenu ToCommand()
    {
        if (Dishes is null || Dishes.Any(dish => dish is null))
            throw new ArgumentException("Add at least one valid dish.");
        return new SaveCuratedMenu(
            Name, Description, ImagePath, PriceJod, MinimumGuests, MaximumGuests,
            Cuisine, Vibe, Event, IsPublished,
            Dishes.Select(dish => new CuratedMenuDishInput(
                dish.Name, dish.ServingDescription, dish.Category)).ToArray());
    }

    public static MenuEditorForm FromMenu(CuratedMenuView menu) => new()
    {
        Name = menu.Name,
        Description = menu.Description,
        ImagePath = menu.ImagePath,
        PriceJod = menu.PriceJod,
        MinimumGuests = menu.MinimumGuests,
        MaximumGuests = menu.MaximumGuests,
        Cuisine = menu.Cuisine,
        Vibe = menu.Vibe,
        Event = menu.Event,
        IsPublished = menu.IsPublished,
        Dishes = menu.Dishes.Select(dish => new MenuDishForm
        {
            Name = dish.Name, ServingDescription = dish.ServingDescription, Category = dish.Category
        }).ToList()
    };
}

public sealed class MenuDishForm
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;
    [Required, StringLength(120)]
    public string ServingDescription { get; set; } = string.Empty;
    [EnumDataType(typeof(MenuCategory))]
    public MenuCategory Category { get; set; }
}
