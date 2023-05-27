using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TesteRuntimeQuery.Models;

namespace TesteRuntimeQuery.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<TestModel> TestModel { get; set; }
        public DbSet<GroupTestingClass> GroupTestingClass { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public string RandomString(int length)
        {
            var randon = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[randon.Next(s.Length)]).ToArray());
        }
    }
}
