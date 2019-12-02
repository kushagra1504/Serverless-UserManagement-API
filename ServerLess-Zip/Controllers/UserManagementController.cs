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
    /// A controller which allows user management to be done
    /// </summary>
    [Route("")] //Making the controller work from Base url
    [ApiController]
    public class UserManagementController : Controller
    {

        private ILogger Logger { get; set; }
        private IUserService UserService { get; set; }

        public UserManagementController( ILogger<UserManagementController> logger, IUserService userService)
        {
            Logger = logger;
            UserService = userService;

        }

        [HttpGet]
        [Route("listusers")]
        [ProducesResponseType(typeof(List<User>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            try
            {
                var users = await UserService.GetAllUsers();
                return Ok(users); ;
            }
            catch (Exception ex)
            {
                Logger.LogError("List users Error", ex.Message);
                return BadRequest();
            }
        }


        [Route("createuser")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> CreateUserAsync([FromBody] User user)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                Logger.LogInformation($"Incoming user: {JsonConvert.SerializeObject(user)}");

                user.EmailAddress = user.EmailAddress.ToLower();
                if (UserService.GetUserByEmail(user.EmailAddress).Result != null)
                {
                    return  BadRequest($"Cannot create user, as user with email {user.EmailAddress} already exists");
                }
                

                await UserService.CreateUser(user);

                return CreatedAtAction(null, user.EmailAddress);
            }

            catch (Exception ex)
            {
                Logger.LogError($"Error occurred when creating user:{user.EmailAddress}.", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getuser/{email}")]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<User>> GetUser(string email)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(email) || !ValidateEmailAddress.IsValidEmailAddress(email))
                {
                    return BadRequest("Please provide a valid email address");
                }

                var user = await UserService.GetUserByEmail(email);
                if (user == null)
                {
                    return NotFound();
                }

                return this.Ok(user);

            }
            catch (Exception ex)
            {
                Logger.LogError($"Error occured when trying to find user :{email} ", ex.Message);
                return BadRequest();
            }
        }

    }
}
