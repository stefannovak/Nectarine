using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using NectarineData.Models;
using Newtonsoft.Json;

namespace NectarineData.DataAccess;

public class NectarineDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public NectarineDbContext(DbContextOptions<NectarineDbContext> opt)
        : base(opt)
    {
    }

    public virtual DbSet<Order> Orders => Set<Order>();

    public virtual DbSet<Product> Products => Set<Product>();

    public virtual DbSet<Rating> Ratings => Set<Rating>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Rating>()
            .HasOne(r => r.User)
            .WithMany(u => u.SubmittedRatings)
            .HasForeignKey(r => r.UserId);

        builder.Entity<Rating>()
            .HasOne(r => r.Product)
            .WithMany(p => p.Ratings)
            .HasForeignKey(r => r.ProductId);

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
