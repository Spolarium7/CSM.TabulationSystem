using CSM.TabulationSystem.Web.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Infrastructure.Data.Helpers
{
    public class DefaultDbContext : DbContext
    {
        public DefaultDbContext(DbContextOptions<DefaultDbContext> options)
    : base(options)
        {
        }
        #region Models
        public DbSet<Contestant> Contestants { get; set; }
        public DbSet<Criterion> Criteria { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Judge> Judges { get; set; }
        public DbSet<EventResult> EventResults { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Score> Scores { get; set; }

        #endregion
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
