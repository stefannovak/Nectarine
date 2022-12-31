using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NectarineAPI.DTOs.Requests;
using NectarineAPI.Models;
using NectarineData.DataAccess;
using NectarineData.Models;

namespace NectarineAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class AddressController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly NectarineDbContext _context;

    public AddressController(
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        NectarineDbContext context)
    {
        _userManager = userManager;
        _mapper = mapper;
        _context = context;
    }

    /// <summary>
    /// Update the user's address.
    /// </summary>
    /// <param name="request">User Address information.</param>
    /// <returns></returns>
    [HttpPut]
    [Route("Update")]
    public async Task<IActionResult> UpdateAddressAsync([FromBody] UpdateAddressDTO request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized(new ApiError("Could not get a user"));
        }

        var previousAddress = user.Addresses.FirstOrDefault(x => x.Id == request.PreviousAddressId);
        if (previousAddress is null)
        {
            return BadRequest(new ApiError(
                $"Could not find an address belonging to the user with the id: {request.PreviousAddressId}"));
        }

        previousAddress.Line1 = request.Address.Line1;
        previousAddress.Line2 = request.Address.Line2;
        previousAddress.City = request.Address.City;
        previousAddress.Postcode = request.Address.Postcode;
        previousAddress.Country = request.Address.Country;
        previousAddress.IsPrimaryAddress = request.Address.IsPrimaryAddress;
        await _context.SaveChangesAsync();

        return Ok();
    }
}