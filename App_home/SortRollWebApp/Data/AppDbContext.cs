using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;

using SortRollWebApp.Models.Entities;

namespace SortRollWebApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Factory> Factories  { get; set; }
        public DbSet<InitialParameters> InitialParameters  { get; set; }
        public DbSet<Pass> Passes { get; set; }
        public DbSet<RollingMill> RollingMills { get; set; }
        public DbSet<RollingStand> RollingStands { get; set; }
        public DbSet<Steel> Steels { get; set; }
        public DbSet<SteelSection> SteelSections { get; set; }
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

    }
}