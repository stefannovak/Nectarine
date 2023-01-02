using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NectarineAPI.DTOs.Generic;
using NectarineAPI.DTOs.Requests;
using NectarineAPI.Models;
using NectarineData.DataAccess;
using NectarineData.Models;

namespace NectarineAPI.Controllers;

[Authorize]
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

    [HttpGet("{id}")]
    public IActionResult GetById([FromQuery] Guid id)
    {
        var userId = _userManager.GetUserId(User);
        var user = _context.Users
            .Include(e => e.UserAddresses)
            .FirstOrDefault(x => x.Id == userId);

        if (user is null)
        {
            return Unauthorized(new ApiError("Could not get a user"));
        }

        var address = user.UserAddresses.FirstOrDefault(x => x.Id == id);
        if (address is null)
        {
            return NotFound(new ApiError($"Could not find an address for the user with the ID {id}"));
        }

        return Ok(_mapper.Map<UserAddressDTO>(address));
    }

    /// <summary>
    /// Get all addresses for the user.
    /// </summary>
    /// <returns></returns>
    [HttpGet("All")]
    public IActionResult GetAll()
    {
        var userId = _userManager.GetUserId(User);
        var user = _context.Users
            .Include(e => e.UserAddresses)
            .FirstOrDefault(x => x.Id == userId);

        if (user is null)
        {
            return Unauthorized(new ApiError("Could not get a user"));
        }

        return Ok(_mapper.Map<IList<UserAddressDTO>>(user.UserAddresses));
    }


    /// <summary>
    /// Create an address for the user.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("Create")]
    public async Task<IActionResult> CreateAddress([FromBody] UserAddressDTO request)
    {
        var userId = _userManager.GetUserId(User);
        var user = _context.Users
            .Include(e => e.UserAddresses)
            .FirstOrDefault(x => x.Id == userId); 
        
        if (user is null)
        {
            return Unauthorized(new ApiError("Could not get a user"));
        }

        var mappedAddress = new UserAddress
        {
            Line1 = request.Line1,
            Line2 = request.Line2,
            City = request.City,
            Country = request.Country,
            Postcode = request.Postcode,
            IsPrimaryAddress = request.IsPrimaryAddress
        };

        if (!request.IsPrimaryAddress)
        {
            user.UserAddresses.Add(mappedAddress);
            await _context.SaveChangesAsync();

            return Ok();
        }
        
        var primaryAddresses = user.UserAddresses.Where(x => x.IsPrimaryAddress);
        primaryAddresses.ToList().ForEach(x =>
        {
            x.IsPrimaryAddress = false;
        });
        
        user.UserAddresses.Add(mappedAddress);
        await _context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Update the user's address.
    /// </summary>
    /// <param name="request">User Address information.</param>
    /// <returns></returns>
    [HttpPut("Update")]
    public async Task<IActionResult> UpdateAddressAsync([FromBody] UpdateAddressDTO request)
    {
        var userId = _userManager.GetUserId(User);
        var user = _context.Users
            .Include(e => e.UserAddresses)
            .FirstOrDefault(x => x.Id == userId); 
        
        if (user is null)
        {
            return Unauthorized(new ApiError("Could not get a user"));
        }

        var previousAddress = user.UserAddresses.FirstOrDefault(x => x.Id == request.PreviousAddressId);
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