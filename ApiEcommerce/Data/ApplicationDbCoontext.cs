using ApiEcommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiEcommerce.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        {
            
        } 
        public DbSet<Category> Categories { get; set; } //It's the representation of the table "Categories" in the database
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
