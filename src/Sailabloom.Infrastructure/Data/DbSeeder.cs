using Microsoft.EntityFrameworkCore;
using Sailabloom.Domain;

namespace Sailabloom.Infrastructure.Data;

/// <summary>
/// Creates the schema (EnsureCreated for the InMemory/Sqlite dev providers)
/// and seeds the starter catalog — the same 8 plants as the live store.
/// Replace names, prices, stock and photos with Sailaja's real inventory
/// from the Admin area.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(SailabloomDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Plants.AnyAsync())
        {
            return;
        }

        context.Plants.AddRange(
            new Plant
            {
                Name = "Monstera Deliciosa",
                Category = "Houseplant",
                Price = 34.00m,
                Stock = 6,
                Description = "The classic statement plant — big, split leaves and easy care. 6-inch pot.",
                ImageUrl = "https://images.unsplash.com/photo-1614594975525-e45190c55d0b?q=80&w=800&auto=format&fit=crop"
            },
            new Plant
            {
                Name = "Snake Plant",
                Category = "Houseplant",
                Price = 24.00m,
                Stock = 10,
                Description = "Nearly indestructible and happy in low light. Perfect first plant. 6-inch pot.",
                ImageUrl = "https://images.unsplash.com/photo-1593482892290-f54927ae1bb6?q=80&w=800&auto=format&fit=crop"
            },
            new Plant
            {
                Name = "Golden Pothos",
                Category = "Trailing",
                Price = 18.00m,
                Stock = 12,
                Description = "Fast-growing trailing vine with golden-variegated leaves. 4-inch pot.",
                ImageUrl = "https://images.unsplash.com/photo-1611211232932-da3113c5b157?q=80&w=800&auto=format&fit=crop"
            },
            new Plant
            {
                Name = "Fiddle-Leaf Fig",
                Category = "Houseplant",
                Price = 58.00m,
                Stock = 4,
                Description = "Tall, sculptural and much loved — bright, indirect light keeps it happiest. 10-inch pot.",
                ImageUrl = "https://images.unsplash.com/photo-1614594895304-fe7116ac2b58?q=80&w=800&auto=format&fit=crop"
            },
            new Plant
            {
                Name = "Peace Lily",
                Category = "Flowering",
                Price = 27.00m,
                Stock = 8,
                Description = "Elegant white blooms and glossy leaves; tells you when it is thirsty. 6-inch pot.",
                ImageUrl = "https://images.unsplash.com/photo-1593691509543-c55fb32d8de5?q=80&w=800&auto=format&fit=crop"
            },
            new Plant
            {
                Name = "Jade Plant",
                Category = "Succulent",
                Price = 16.00m,
                Stock = 15,
                Description = "A lucky little succulent tree that thrives on neglect. 4-inch pot.",
                ImageUrl = "https://images.unsplash.com/photo-1509587584298-0f3b3a3a1797?q=80&w=800&auto=format&fit=crop"
            },
            new Plant
            {
                Name = "Boston Fern",
                Category = "Fern",
                Price = 22.00m,
                Stock = 7,
                Description = "Lush, feathery fronds for hanging baskets and shady corners. 6-inch pot.",
                ImageUrl = "https://images.unsplash.com/photo-1463154545680-d59320fd685d?q=80&w=800&auto=format&fit=crop"
            },
            new Plant
            {
                Name = "English Lavender",
                Category = "Herb",
                Price = 15.00m,
                Stock = 9,
                Description = "Fragrant purple blooms for sunny beds, borders and pots. 4-inch pot.",
                ImageUrl = "https://images.unsplash.com/photo-1499002238440-d264edd596ec?q=80&w=800&auto=format&fit=crop"
            });

        await context.SaveChangesAsync();
    }
}
