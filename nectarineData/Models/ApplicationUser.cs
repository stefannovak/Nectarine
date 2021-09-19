using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace nectarineData.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required] 
        [MaxLength(100)] 
        public string StripeCustomerId { get; set; } = null!;
    }
}