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

        public DbSet<UserAddress> UserAddresses { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // builder.Entity<ApplicationUser>()
            //     .HasMany(u => u.UserAddresses)
            //     .WithOne(a => a.User)
            //     .IsRequired()
            //     .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Order>()
                .Property(x => x.ProductIds)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>())
                .Metadata.SetValueComparer(new ValueComparer<ICollection<string>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode()))));
        }
    }
}
