using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TK2017MTAADv2.Models
{
    public class TenantContext:DbContext
    {
        public TenantContext(DbContextOptions<TenantContext> options) : base(options)
        {
        }
        public DbSet<Tenant> Tenants { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tenant>().ToTable("tblTenat");
            base.OnModelCreating(modelBuilder);
        }
    }
    public class Tenant
    {
        /// <summary>
        /// PK
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Guid with tenant ID
        /// </summary>
        public string TenantGuid { get; set; }
        /// <summary>
        /// Secret to check 
        /// </summary>
        public string Secret { get; set; }

        public string GroupGuid { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime DtCreated { get; set; }
    }
}
