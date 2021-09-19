using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using nectarineAPI.Controllers;
using nectarineAPI.DTOs.Generic;
using nectarineData.Models;
using Xunit;

namespace nectarineTests.Controllers
{
    public class UsersControllerTest
    {
        private readonly UsersController _controller;
        private readonly Mock<IMapper> _mockMapper = new ();

        public UsersControllerTest()
        {
            // UserManager setup
            var store = new Mock<IUserStore<ApplicationUser>>();
            Mock<UserManager<ApplicationUser>> _userManager = new (store.Object, null, null, null, null, null, null, null, null);
            
            _userManager.Setup(manager => manager
                    .GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser());
            
            // AutoMapper setup
            _mockMapper.Setup(x => x.Map<UserDTO>(It.IsAny<ApplicationUser>()))
                .Returns((ApplicationUser source) => new UserDTO
                {
                    Id = source.Id,
                    Email = source.Email,
                });
            
            _controller = new UsersController(_userManager.Object, _mockMapper.Object);
        }
        
        [Fact(DisplayName = "GetCurrent should get the current user and return an Ok")]
        public async Task Test_GetCurrentTest()
        {
            // Act
            var result = await _controller.GetCurrent();
            
            // Arrange
            Assert.IsType<OkObjectResult>(result);
        }

    }
}