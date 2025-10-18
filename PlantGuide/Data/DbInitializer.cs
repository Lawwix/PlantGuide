using PlantGuide.Data;
using PlantGuide.Models;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        if (context.Plants.Any()) return;

        var plants = new[] {
            new Plant { Name = "Фикус", ScientificName = "Ficus elastica", 
                Description = "Универсальное комнатное растение", 
                CareInstructions = "Яркий рассеянный свет, умеренный полив",
                PhotoPath = "images/ficus.jpeg"},
            new Plant { Name = "Сансевиерия", ScientificName = "Sansevieria trifasciata", 
                Description = "Неприхотливое", 
                CareInstructions = "Свет — от тени до яркого; редкий полив",
                PhotoPath = "images/sanseveriya.jpeg"}
        };

        context.Plants.AddRange(plants);
        context.SaveChanges();
    }
}
