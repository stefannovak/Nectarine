using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NectarineAPI.DTOs.Generic;
using NectarineAPI.Models;
using NectarineData.DataAccess;
using NectarineData.Models;

namespace NectarineAPI.Controllers;

[Authorize]
[Route("[controller]")]
[ApiController]
public class RatingController : ControllerBase
{
    private readonly NectarineDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public RatingController(
        NectarineDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Create a rating by a User for a Product.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(UnauthorizedResult), 401)]
    [ProducesResponseType(typeof(NoContentResult), 204)]
    public async Task<IActionResult> CreateRating([FromBody] RatingDTO request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var rating = new Rating(request.Stars, request.Review, request.ProductId)
        {
            User = user,
        };

        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Delete a rating belonging to the User.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(UnauthorizedResult), 401)]
    [ProducesResponseType(typeof(NotFoundObjectResult), 404)]
    [ProducesResponseType(typeof(NoContentResult), 204)]
    public async Task<IActionResult> DeleteRating(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        var user = _context.Users
            .Include(e => e.SubmittedRatings)
            .FirstOrDefault(x => x.Id == userId);

        if (user is null)
        {
            return Unauthorized();
        }

        var rating = user.SubmittedRatings.FirstOrDefault(x => x.Id == id);
        if (rating is null)
        {
            return NotFound(new ApiError($"Could not find a rating belonging to user with the id: {id}"));
        }

        _context.Ratings.Remove(rating);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}