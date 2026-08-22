using Application.Common.DTO;
using Application.Contracts.Persistance;
using Application.Contracts.Services.Auth;
using Application.Services.Auth;
using Domain.Exceptions.Auth;
using Domain.Exceptions.Booking;
using Domain.Models;
using Domain.Options;
using Microsoft.Extensions.Options;
using Moq;
using Services = Application.Services.Auth;

namespace EventBrokerAPI.Tests.AuthenticationService
{
    public class Test
    {
        [Fact]
        public async Task RegisterUser_WhenUserExists_ReturnsFalse()
        {
            // Arrange
            var userName = "existing";
            var mockUserRepo = new Mock<IUserRepository>();
            mockUserRepo.Setup(r => r.GetUserByNameAsync(userName))
                        .ReturnsAsync(User.Restore(Guid.CreateVersion7(), userName, "hash"));

            var mockRepo = new Mock<IRepositoryManager>();
            mockRepo.Setup(r => r.User).Returns(mockUserRepo.Object);

            var mockHash = new Mock<IHashService>();
            var mockOptions = new Mock<IOptionsSnapshot<JwtSettings>>();
            mockOptions.Setup(o => o.Value).Returns(new JwtSettings { Secret = "secret", ValidAudience = "aud", ValidIssuer = "iss", ExpiresMinutes = 60 });

            var service = new Services.AuthenticationService(mockRepo.Object, mockHash.Object, mockOptions.Object);

            // Act
            await Assert.ThrowsAsync <WhoAreYouException> (() => service.RegisterUser(new UserRegisterDTO { UserName = userName, Password = "pwd", Role = Role.User }));
            mockUserRepo.Verify(r => r.CreateUser(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ValidateUser_WhenPasswordMatches_ReturnsToken()
        {
            // Arrange
            var hashService = new HashService();

            var userName = "test";
            var password = "pwd";
            var storedHash = hashService.Hash(password);

            var user = User.Restore(Guid.CreateVersion7(), userName, storedHash, Role.User);


            var mockUserRepo = new Mock<IUserRepository>();
            mockUserRepo.Setup(r => r.GetUserByNameAsync(userName)).ReturnsAsync(user);

            var mockRepo = new Mock<IRepositoryManager>();
            mockRepo.Setup(r => r.User).Returns(mockUserRepo.Object);

            var mockOptions = new Mock<IOptionsSnapshot<JwtSettings>>();
            mockOptions.Setup(o => o.Value).Returns(new JwtSettings { Secret = "verysecretkeyasdfdsasdfcfdsfd1234567890", ValidAudience = "aud", ValidIssuer = "iss", ExpiresMinutes = 60 });

            var service = new Services.AuthenticationService(mockRepo.Object, hashService, mockOptions.Object);

            // Act
            var result = await service.ValidateUser(new UserLoginDTO { UserName = userName, Password = password });

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(string.IsNullOrEmpty(result.Token));
        }
    }
}
