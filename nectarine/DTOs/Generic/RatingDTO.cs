using System;
using System.ComponentModel.DataAnnotations;

namespace NectarineAPI.DTOs.Generic;

public class RatingDTO
{
    public RatingDTO(
        uint stars,
        string? review,
        Guid productId)
    {
        Stars = stars;
        Review = review;
        ProductId = productId;
    }

    public Guid Id { get; set; }

    [Range(1, 5)]
    public uint Stars { get; set; }

    [MaxLength(500)]
    public string? Review { get; set; }

    public Guid ProductId { get; set; }
}