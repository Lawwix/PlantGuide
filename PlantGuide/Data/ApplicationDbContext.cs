using Microsoft.EntityFrameworkCore;
using PlantGuide.Models;

namespace PlantGuide.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Plant> Plants { get; set; }
    }
}
