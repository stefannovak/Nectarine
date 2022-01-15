using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NectarineData.Models.Enums;

namespace NectarineData.Models
{
    public class ExternalAuthLink
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(512)]
        public string PlatformId { get; set; } = string.Empty;

        [Required] 
        public ExternalAuthPlatform Platform { get; set; }
    }
}