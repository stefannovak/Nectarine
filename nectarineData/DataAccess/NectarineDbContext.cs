using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using nectarineData.Models;

namespace nectarineData.DataAccess
{
    public class NectarineDbContext : IdentityDbContext<ApplicationUser>
    {
        public NectarineDbContext(DbContextOptions<NectarineDbContext> opt)
            : base(opt)
        {
        }
    }
}
