using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context
{

    public class ApplicationContext : DbContext
    {

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

        public virtual DbSet<User> User { get; set; }
    }
}
