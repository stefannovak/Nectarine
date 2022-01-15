using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NectarineData.Models;

namespace NectarineData.DataAccess
{
    public class NectarineDbContext : IdentityDbContext<ApplicationUser>
    {
        public NectarineDbContext(DbContextOptions<NectarineDbContext> opt)
            : base(opt)
        {
        }
    }
}
