using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NectarineData.Models;
using Newtonsoft.Json;

namespace NectarineData.DataAccess
{
    public class NectarineDbContext : IdentityDbContext<ApplicationUser>
    {
        public NectarineDbContext(DbContextOptions<NectarineDbContext> opt)
            : base(opt)
        {
        }

        public virtual DbSet<Order> Orders => Set<Order>();

        public virtual DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Order>()
                .Property(x => x.ProductIds)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v))
                .Metadata.SetValueComparer(new ValueComparer<ICollection<string>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode()))));
        }
    }
}
