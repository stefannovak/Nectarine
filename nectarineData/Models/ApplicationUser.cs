using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace nectarineData.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required] 
        [MaxLength(100)] 
        public string StripeCustomerId { get; set; } = string.Empty;
        
        [Required] 
        [MaxLength(100)] 
        public string FirstName { get; set; } = string.Empty;
        
        [Required] 
        [MaxLength(100)] 
        public string LastName { get; set; } = string.Empty;

        [Required] 
        public virtual IList<ExternalAuthLink> SocialLinks { get; set; } = new List<ExternalAuthLink>();
    }
}