using System;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NectarineAPI.Controllers;
using NectarineAPI.DTOs.Generic;
using NectarineData.DataAccess;
using NectarineData.Models;

namespace NectarineTests.Controllers.RatingsControllerTests;

public partial class RatingsControllerTests
{
    private readonly RatingController _subject;
    private readonly Mock<IMapper> _mockMapper = new ();
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly NectarineDbContext _mockContext;
    private readonly MockHelpers _mockHelpers = new ();

    private readonly ApplicationUser _user = new ()
    {
        Id = Guid.NewGuid().ToString(),
        Email = "test@gmail.com",
    };

    private readonly RatingDTO _testRatingDto;

    private readonly Product _product = new (
        name: "Black shirt",
        description: "What an amazing shirt",
        primaryColorName: "pink",
        size: "L",
        price: 1000,
        material: "lace",
        sex: "M",
        category: "shirt",
        image: "https://www.uniqlo.com/jp/ja/contents/feature/masterpiece/common_22ss/img/products/contentsArea_itemimg_16.jpg");

    public RatingsControllerTests()
    {
        _testRatingDto = new (5, "Amazing", _product.Id);

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
        _mockMapper.Setup(x => x.Map<Rating>(It.IsAny<RatingDTO>()))
            .Returns((RatingDTO source) => new Rating(source.Stars, source.Review, source.ProductId));

        // Database setup
        var options = new DbContextOptionsBuilder<NectarineDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        _mockContext = new NectarineDbContext(options);
        _mockContext.Users.Add(_user);
        _mockContext.SaveChanges();

        _subject = new RatingController(
            _mockContext,
            _mockMapper.Object,
            _userManager.Object);
    }
}