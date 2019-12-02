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
    public class AccountManagementControllerTests
    {

        private AccountRequest GetAccountTestData()
        {
            return new AccountRequest
            {
                EmailAddress = "kush@kushagra.ga",
                CreditRequested = 799.10
            };
        }

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


        private User GetUnmatchingUserTestData()
        {
            return new User
            {
                EmailAddress = "nomatch@kushagra.ga",
                MonthlySalary = 15000,
                MonthlyExpenses = 7500,
                Name = "Nomatch"
            };
        }
        [Fact]
        public async Task account_management_controller_getaccounts_calls_service_getallaccounts()
        {
            var accountServiceMock = new Mock<IAccountService>();
            var loggerServiceMock = new Mock<ILogger<AccountManagementController>>();
            var userServiceMock = new Mock<IUserService>();
            accountServiceMock.Setup(svc => svc.GetAllAccounts()).ReturnsAsync(new List<Account>());
            var controller = new AccountManagementController(loggerServiceMock.Object, userServiceMock.Object, accountServiceMock.Object);

            var result = await controller.GetAccounts();
            result.Should().NotBeNull();

            accountServiceMock.Verify(svc => svc.GetAllAccounts(), Times.Once);
        }

   

        [Fact]
        public async Task account_management_controller_create_account_calls_service_create()
        {
            var testAccountData = GetAccountTestData();
            var accountServiceMock = new Mock<IAccountService>();
            var userServiceMock = new Mock<IUserService>();
            var loggerServiceMock = new Mock<ILogger<AccountManagementController>>();
            userServiceMock.Setup(svc => svc.GetUserByEmail(testAccountData.EmailAddress))
                                                .ReturnsAsync(GetUserTestData());
            var controller = new AccountManagementController(loggerServiceMock.Object, userServiceMock.Object, accountServiceMock.Object);

            var result = await controller.CreateAccountAsync(testAccountData);

            var contentResult = result as ObjectResult;
            contentResult.Should().NotBeNull();
        }

        [Fact]
        public async Task account_management_controller_create_account_without_user_gives_error()
        {
            var testAccountData = GetAccountTestData();
            var accountServiceMock = new Mock<IAccountService>();
            var userServiceMock = new Mock<IUserService>();
            var loggerServiceMock = new Mock<ILogger<AccountManagementController>>();
            userServiceMock.Setup(svc => svc.GetUserByEmail(GetUnmatchingUserTestData().EmailAddress))
                                                .ReturnsAsync(GetUnmatchingUserTestData());
            var controller = new AccountManagementController(loggerServiceMock.Object, userServiceMock.Object, accountServiceMock.Object);

            var result = await controller.CreateAccountAsync(testAccountData);

            var contentResult = result as ObjectResult;
            contentResult.Should().NotBeNull();

            userServiceMock.Verify(svc => svc.GetUserByEmail(testAccountData.EmailAddress), Times.Once);
            accountServiceMock.Verify(svc => svc.CreateAccount(testAccountData, GetUserTestData()), Times.Once);
        }
    }
}
