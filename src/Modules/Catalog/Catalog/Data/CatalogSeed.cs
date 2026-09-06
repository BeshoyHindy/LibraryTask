using Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Data;

public static class CatalogSeed
{
    // Ids come from the database, so every read builds fresh entities rather than handing out
    // instances an earlier insert has already stamped.
    public static IReadOnlyList<Book> Books =>
    [
        new() { Title = "The Silent Cartographer", Author = "Mira Okonkwo", Pages = 312 },
        new() { Title = "Salt and Longitude", Author = "Henrik Vasser", Pages = 448 },
        new() { Title = "A Short History of Rain", Author = "Priya Raman", Pages = 186 },
        new() { Title = "The Glassblower's Apprentice", Author = "Elena Ferraro", Pages = 524 },
        new() { Title = "Notes on a Quiet Harbour", Author = "Tomas Lindqvist", Pages = 134 },
        new() { Title = "The Cartwright Inheritance", Author = "Adaeze Nwosu", Pages = 672 },
        new() { Title = "Meridian Nine", Author = "Sofia Marchetti", Pages = 1024 },
        new() { Title = "Fieldwork in the Ash Barrens", Author = "Daniel Ochoa", Pages = 398 },
        new() { Title = "The Lantern Keepers", Author = "Yuki Tanabe", Pages = 256 },
        new() { Title = "Every Bridge in Prague", Author = "Marek Dvorak", Pages = 1180 },
        new() { Title = "The Weight of Small Machines", Author = "Clara Bergstrom", Pages = 104 },
        new() { Title = "Almanac of Forgotten Roads", Author = "Idris Haddad", Pages = 742 },
    ];

    public static async Task ApplyAsync(CatalogDbContext dbContext, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        if (await dbContext.Books.AnyAsync(cancellationToken))
        {
            return;
        }

        dbContext.Books.AddRange(Books);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
