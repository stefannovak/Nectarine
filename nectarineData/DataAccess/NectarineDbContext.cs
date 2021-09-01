using System;
using Microsoft.EntityFrameworkCore;
using nectarineData.Models;

namespace nectarineData.DataAccess
{
    public class NectarineDbContext : DbContext
    {
        public NectarineDbContext(DbContextOptions<NectarineDbContext> opt) : base(opt)
        {
        }
        
        public virtual DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    }
}
