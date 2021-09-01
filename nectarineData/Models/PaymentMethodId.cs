using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nectarineData.Models
{
    public class PaymentMethodId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        [Required] 
        [MaxLength(100)] 
        public string TokenId { get; set; } = null!;
    }
}