using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using nectarineAPI.DTOs.Generic;
using nectarineData.Models;

namespace nectarineAPI.Controllers
{
    [Microsoft.AspNetCore.Components.Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public UsersController(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }
        
        /// <summary>
        /// Returns the current user from their <see cref="ControllerBase"/> Claims Principle.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCurrent")]
        public async Task<IActionResult> GetCurrent() => Ok(_mapper.Map<UserDTO>(await _userManager.GetUserAsync(User)));
    }
}