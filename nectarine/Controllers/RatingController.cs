using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NectarineAPI.DTOs.Generic;
using NectarineData.DataAccess;
using NectarineData.Models;

namespace NectarineAPI.Controllers;

[Authorize]
[Route("[controller]")]
[ApiController]
public class RatingController : ControllerBase
{
    private readonly NectarineDbContext _context;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public RatingController(
        NectarineDbContext context,
        IMapper mapper,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
    }

    [HttpPost]
    [ProducesResponseType(typeof(NoContentResult), 204)]
    [ProducesResponseType(typeof(UnauthorizedResult), 401)]
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
}