using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerLess_Zip.Model;
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
        private IAmazonDynamoDB DDBClient { get; set; }
        private ILogger Logger { get; set; }
        private IDynamoDBContext DDBContext { get; set; }
        private string UserTableName { get; set; }

        private static bool IsValidEmailAddress(string emailAddress)
        {
            return new System.ComponentModel.DataAnnotations
                                .EmailAddressAttribute()
                                .IsValid(emailAddress);
        }

        public UserManagementController(IConfiguration configuration, ILogger<UserManagementController> logger, IAmazonDynamoDB ddbClient)
        {
            Logger = logger;
            DDBClient = ddbClient;

            UserTableName = configuration[Startup.AppDDBTableKey];
            if (string.IsNullOrEmpty(UserTableName))
            {
                Logger.LogCritical("Missing configuration for DDB UserTable. The AppDDBTable configuration must be set to a DDB UserTable.");
                throw new Exception("Missing configuration for DDB UserTable. The AppDDBTable configuration must be set to a DDB UserTable.");
            }
            else
            {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(User)] = new Amazon.Util.TypeMapping(typeof(User), UserTableName);
            }

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            DDBContext = new DynamoDBContext(ddbClient, config);

            logger.LogInformation($"Configured to use the table: {UserTableName}");
        }

        [HttpGet]
        [Route("listusers")]
        [ProducesResponseType(typeof(List<User>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> GetUsers()
        {
            try
            {
                Logger.LogDebug("Getting the users");
                var search = DDBContext.ScanAsync<User>(null);
                var page = await search.GetNextSetAsync();
                Logger.LogDebug($"Found {page.Count} users");
                return Ok(page); ;
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
                user.EmailAddress = user.EmailAddress.ToLower();
                Logger.LogInformation($"Incoming user object: {JsonConvert.SerializeObject(user)}");

                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                Logger.LogInformation($"Creation for User: {user.EmailAddress} started.");
                Logger.LogInformation($"Saving User: {user.EmailAddress}");

                await DDBContext.SaveAsync<User>(user);

                return CreatedAtAction(null, user.EmailAddress);
            }

            catch (Exception ex)
            {
                Logger.LogError($"Error occurred when creating user:{user.EmailAddress}.", ex.Message);
                return BadRequest();
            }
        }

        [HttpGet("getuser/{email}")]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<User>> GetUser(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || !IsValidEmailAddress(email))
                {
                    return BadRequest("Please provide a valid email address");
                }
                

                Logger.LogInformation($"Getting user details with {email}.");

                var user = await DDBContext.LoadAsync<User>(email.ToLower());

                Logger.LogInformation($"Found user: {user != null}");

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
