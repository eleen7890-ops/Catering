using Domain;

namespace Web.Models;

public static class HomePageSamples
{
    public static IReadOnlyList<OccasionCard> Occasions { get; } =
    [
        new("Cozy Movie Night", "Comfort food, good company, one more episode.", "STAY IN", "sage", Mood.Cozy),
        new("Friends’ Brunch", "Slow mornings and a table worth lingering at.", "SLOW DOWN", "peach", Event: EventType.PrivateGathering),
        new("Birthday Party", "Make a wish. We’ll bring the delicious part.", "CELEBRATE", "butter", Event: EventType.Birthday),
        new("Family Gathering", "The kind of spread everyone remembers.", "COME TOGETHER", "clay", Event: EventType.PrivateGathering),
        new("Elegant Dinner", "A little candlelight, a lot of flavor.", "DRESS UP", "olive", Mood.Elegant)
    ];
}
