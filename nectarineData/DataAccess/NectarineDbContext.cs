using System;
using Microsoft.EntityFrameworkCore;

namespace nectarineData.DataAccess
{
    public class NectarineDbContext : DbContext
    {
        public NectarineDbContext(DbContextOptions<NectarineDbContext> opt) : base(opt)
        {
        }
    }
}
