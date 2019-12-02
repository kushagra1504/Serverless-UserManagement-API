using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ServerLess_Zip.Controllers;
using ServerLess_Zip.Model;
using ServerLess_Zip.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace ServerLess_Zip.Tests
{
    public class UserManagementControllerTests
    {


        private User GetUserTestData()
        {
            return new User
            {
                EmailAddress = "kush@kushagra.ga",
                MonthlySalary = 15000,
                MonthlyExpenses = 7500,
                Name = "Kushagra"
            };
        }

        [Fact]
        public async Task user_management_controller_getusers_calls_service_getallusers()
        {
            var loggerServiceMock = new Mock<ILogger<UserManagementController>>();
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(svc => svc.GetAllUsers()).ReturnsAsync(new List<User>());
            var controller = new UserManagementController(loggerServiceMock.Object, userServiceMock.Object);

            var result = await controller.GetUsers();
            result.Should().NotBeNull();

            userServiceMock.Verify(svc => svc.GetAllUsers(), Times.Once);
        }


        [Fact]
        public async Task user_management_controller_create_users_calls_service_create()
        {
            var testAccountData = GetUserTestData();
            var userServiceMock = new Mock<IUserService>();
            var loggerServiceMock = new Mock<ILogger<UserManagementController>>();
            var controller = new UserManagementController(loggerServiceMock.Object, userServiceMock.Object);

            var result = await controller.CreateUserAsync(testAccountData);

            var contentResult = result as ObjectResult;
            contentResult.Should().NotBeNull();

            userServiceMock.Verify(svc => svc.GetUserByEmail(testAccountData.EmailAddress), Times.Once);
        }

    }
}
