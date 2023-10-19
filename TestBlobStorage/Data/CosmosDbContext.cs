using Microsoft.EntityFrameworkCore;
using TestBlobStorage.Models;

namespace TestBlobStorage.Data
{
    public class CosmosDbContext:DbContext
    {
        public CosmosDbContext(DbContextOptions options) : base(options) { }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToContainer("users").HasPartitionKey(e=>e.Id);

        }

        public DbSet<User>? Users { get; set; }
    }
}
