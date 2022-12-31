using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NectarineAPI.Controllers;
using NectarineAPI.DTOs.Generic;
using NectarineData.DataAccess;
using NectarineData.Models;

namespace NectarineTests.Controllers.AddressControllerTests;

public partial class AddressControllerTests
{
    private readonly AddressController _subject;
    private readonly Mock<IMapper> _mockMapper = new ();
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly NectarineDbContext _mockContext;

    private readonly ApplicationUser _user = new ()
    {
        PhoneNumber = "123123123123",
        VerificationCode = 123123,
        PhoneNumberConfirmed = false,
    };

    public AddressControllerTests()
    {
        // UserManager setup
        _userManager = MockHelpers.MockUserManager<ApplicationUser>();

        _userManager
            .Setup(manager => manager
                .GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_user);

        _userManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .Returns<string>((email) => Task.FromResult(new ApplicationUser { Email = email }));

        _userManager
            .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // AutoMapper setup
        _mockMapper.Setup(x => x.Map<UserDTO>(It.IsAny<ApplicationUser>()))
            .Returns((ApplicationUser source) => new UserDTO
            {
                Id = source.Id,
                Email = source.Email,
            });

        // Database setup
        var options = new DbContextOptionsBuilder<NectarineDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        _mockContext = new NectarineDbContext(options);

        _subject = new AddressController(
            _userManager.Object,
            _mockMapper.Object,
            _mockContext);
    }
}