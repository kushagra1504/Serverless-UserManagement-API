using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerLess_Zip.Model;
using ServerLess_Zip.Services;
using ServerLess_Zip.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ServerLess_Zip.Controllers
{
    /// <summary>
    /// A controller which allows Account management to be done
    /// </summary>
    [Route("")] //Making the controller work from Base url
    [ApiController]
    public class AccountManagementController : Controller
    {
       
        private ILogger Logger { get; set; }
        private IUserService UserService { get; set; }
        private IAccountService AccountService { get; set; }
      

        public AccountManagementController(ILogger<AccountManagementController> logger, IUserService userService, IAccountService accountService)
        {
            Logger = logger;
            UserService = userService;
            AccountService = accountService;
        }

        [HttpGet]
        [Route("listaccounts")]
        [ProducesResponseType(typeof(List<Account>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<List<Account>>> GetAccounts()
        {
            try
            {
                var accounts = await AccountService.GetAllAccounts();
                return Ok(accounts); ;
            }
            catch (Exception ex)
            {
                Logger.LogError("List Accounts Error", ex.Message);
                return BadRequest();
            }
        }

        [Route("createaccount")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> CreateAccountAsync([FromBody] AccountRequest accountRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                Logger.LogInformation($"Incoming account request: {JsonConvert.SerializeObject(accountRequest)}");

                accountRequest.EmailAddress = accountRequest.EmailAddress.ToLower();
                var user = UserService.GetUserByEmail(accountRequest.EmailAddress).Result;
                if (user == null)
                {
                    return BadRequest($"Cannot create account, as user with email {accountRequest.EmailAddress} doesn't exists in the system. Please create a valid user first.");
                }

                if (user.MonthlySalary - user.MonthlyExpenses < 1000 )
                {
                    return BadRequest($"Cannot create account, as user with email {accountRequest.EmailAddress} has high monthly expenses.");
                }


                await AccountService.CreateAccount(accountRequest, user);

                return CreatedAtAction(null, user.EmailAddress);
            }

            catch (Exception ex)
            {
                Logger.LogError($"Error occurred when creating account for user:{accountRequest.EmailAddress}.", ex.Message);
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("getaccount/{email}")]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<User>> GetAccount(string email)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(email) || !ValidateEmailAddress.IsValidEmailAddress(email))
                {
                    return BadRequest("Please provide a valid email address");
                }

                var account = await AccountService.GetAccountByEmail(email);
                if (account == null)
                {
                    return NotFound();
                }

                return this.Ok(account);

            }
            catch (Exception ex)
            {
                Logger.LogError($"Error occured when trying to find account :{email} ", ex.Message);
                return BadRequest();
            }
        }

    }
}
