using Moq;
using NetBackend.Controllers;
using NetBackend.Services.Interfaces.Keys;
using NetBackend.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NetBackend.Services.Interfaces;
using Microsoft.Extensions.Logging;
using NetBackend.Models.Dto.Keys;
using Netbackend.Models.Dto.Keys;
using NetBackend.Models.Keys;

namespace NetBackend.Tests.Controllers
{
    public class RestControllerTests
    {
        private readonly Mock<ILogger<RestController>> _loggerMock = new();
        private readonly Mock<IRestKeyService> _restKeyServiceMock = new();
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<IKafkaProducerService> _kafkaProducerServiceMock = new();
        [Fact]
        public async Task CreateAccessKey_ReturnsOk()
        {
            // Arrange
            var user = new UserModel { Id = "user123", Email = "test@example.com" };
            var createRestAccessKeyDto = new CreateRestAccessKeyDto { KeyName = "TestKey", ThemeIds = [] };

            _userServiceMock.Setup(s => s.GetUserByHttpContextAsync(It.IsAny<HttpContext>())).ReturnsAsync((user, null));
            _restKeyServiceMock.Setup(s => s.CreateRESTApiKey(It.IsAny<UserModel>(), It.IsAny<string>(), It.IsAny<List<Guid>>())).ReturnsAsync(new RestApiKey { KeyName = "TestKey", UserId = "user123", IsEnabled = true, User = user });

            var controller = new RestController(_loggerMock.Object, _restKeyServiceMock.Object, _userServiceMock.Object, _kafkaProducerServiceMock.Object);

            // Act
            var result = await controller.CreateAccessKey(createRestAccessKeyDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult?.Value);
            Assert.IsType<AccessKeyDto>(okResult?.Value);
        }
    }
}
