using System;
using System.Collections.Generic;
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
    private readonly MockHelpers _mockHelpers = new ();

    private readonly ApplicationUser _user = new ()
    {
        Id = Guid.NewGuid().ToString(),
        PhoneNumber = "123123123123",
        VerificationCode = 123123,
        PhoneNumberConfirmed = false,
        UserAddresses = new List<UserAddress>
        {
            new ("Test", null, "London", "12312", "UK", true),
        },
    };

    private readonly UserAddressDTO _testDto =
        new("Test", null, "London", "12312", "UK", true);

    public AddressControllerTests() 
    {
        // UserManager setup
        _userManager = MockHelpers.MockUserManager<ApplicationUser>();

        _userManager
            .Setup(manager => manager
                .GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_user);

        _userManager
            .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(_user.Id);

        // AutoMapper setup
        _mockMapper.Setup(x => x.Map<IList<UserAddressDTO>>(It.IsAny<ICollection<UserAddress>>()))
            .Returns((ICollection<UserAddress> source) => new List<UserAddressDTO>
            {
                _testDto,
            });
        
        _mockMapper.Setup(x => x.Map<UserAddress>(It.IsAny<UserAddressDTO>()))
            .Returns((UserAddressDTO source) =>
                new UserAddress(source.Line1, source.Line2, source.City, source.Country, source.Postcode));

        // Database setup
        var options = new DbContextOptionsBuilder<NectarineDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        _mockContext = new NectarineDbContext(options);
        _mockContext.Users.Add(_user);
        _mockContext.SaveChanges();

        _subject = new AddressController(
            _userManager.Object,
            _mockMapper.Object,
            _mockContext);
    }
}