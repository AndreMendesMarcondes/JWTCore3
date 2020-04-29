using JWTCore3.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JWTCore3.Data
{
    public class DemoTokenContext : IdentityDbContext
    {
        public DemoTokenContext(DbContextOptions<DemoTokenContext> options)
            : base(options)
        {

        }

        public DbSet<Product> Product { get; set; }
    }
}
