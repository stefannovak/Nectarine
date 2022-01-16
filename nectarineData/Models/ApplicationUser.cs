using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NectarineData.Models
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

        public int? VerificationCode { get; set; }

        public DateTime? VerificationCodeExpiry { get; set; }

        [Required]
        public virtual IList<ExternalAuthLink> SocialLinks { get; set; } = new List<ExternalAuthLink>();
    }
}