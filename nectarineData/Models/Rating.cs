using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NectarineData.Models;

public class Rating
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Rating"/> class.
    /// Not to be used.
    /// </summary>
    public Rating()
    {
    }

    public Rating(
        uint stars,
        string? review,
        // string userId,
        Guid productId)
    {
        Stars = stars;
        Review = review;
        // UserId = userId;
        ProductId = productId;
    }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    [Required]
    [Range(1, 5)]
    public uint Stars { get; set; }

    [MaxLength(500)]
    public string? Review { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public Guid ProductId { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}