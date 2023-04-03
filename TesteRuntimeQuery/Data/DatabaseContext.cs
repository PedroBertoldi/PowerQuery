using Microsoft.EntityFrameworkCore;
using TesteRuntimeQuery.Models;

namespace TesteRuntimeQuery.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<TestModel> TestModel { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
    }
}
