using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NectarineData.Models;

namespace NectarineTests;

public class MockHelpers
{
    public static StringBuilder LogMessage = new ();

    public static Mock<UserManager<TUser>> MockUserManager<TUser>()
        where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var passwordHashser = new Mock<IPasswordHasher<TUser>>();
        var mgr = new Mock<UserManager<TUser>>(
            store.Object,
            null,
            passwordHashser.Object,
            null,
            null,
            null,
            null,
            null,
            null);
        mgr.Object.UserValidators.Add(new UserValidator<TUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

        mgr.Setup(x => x.DeleteAsync(It.IsAny<TUser>()))
            .ReturnsAsync(IdentityResult.Success);
        mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        mgr.Setup(x => x.UpdateAsync(It.IsAny<TUser>()))
            .ReturnsAsync(IdentityResult.Success);
        passwordHashser.Setup(x => x.VerifyHashedPassword(
                It.IsAny<TUser>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(PasswordVerificationResult.Success);

        return mgr;
    }

    public static UserManager<TUser> TestUserManager<TUser>(IUserStore<TUser> store = null!)
        where TUser : class
    {
        var options = new Mock<IOptions<IdentityOptions>>();
        var idOptions = new IdentityOptions();
        idOptions.Lockout.AllowedForNewUsers = false;
        options.Setup(o => o.Value).Returns(idOptions);
        var userValidators = new List<IUserValidator<TUser>>();
        var validator = new Mock<IUserValidator<TUser>>();
        userValidators.Add(validator.Object);
        var pwdValidators = new List<PasswordValidator<TUser>>();
        pwdValidators.Add(new PasswordValidator<TUser>());
        var userManager = new UserManager<TUser>(
            store,
            options.Object,
            new PasswordHasher<TUser>(),
            userValidators,
            pwdValidators,
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null,
            new Mock<ILogger<UserManager<TUser>>>().Object);
        validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<TUser>()))
            .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();
        return userManager;
    }

    public void UserManager_ReturnsRandomId(Mock<UserManager<ApplicationUser>> userManager)
    {
        userManager
            .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(Guid.NewGuid().ToString);
    }

    public void UserManager_GetUserAsync_ReturnsNothing(Mock<UserManager<ApplicationUser>> userManager)
    {
        userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()));
    }
}