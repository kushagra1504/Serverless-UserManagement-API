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
    /// A controller which allows Account management to be done
    /// </summary>
    [Route("accounts")] //Making the controller work from Base url
    [ApiController]
    public class AccountManagementController : Controller
    {
        private IAmazonDynamoDB DDBClient { get; set; }
        private ILogger Logger { get; set; }
        private IDynamoDBContext DDBContext { get; set; }
        private string AccountTableName { get; set; }

        private static bool IsValidEmailAddress(string emailAddress)
        {
            return new System.ComponentModel.DataAnnotations
                                .EmailAddressAttribute()
                                .IsValid(emailAddress);
        }

        public AccountManagementController(IConfiguration configuration, ILogger<AccountManagementController> logger, IAmazonDynamoDB ddbClient)
        {
            Logger = logger;
            DDBClient = ddbClient;

            AccountTableName = configuration[Startup.AppDDBAccountTableKey];
            if (string.IsNullOrWhiteSpace(AccountTableName))
            {
                Logger.LogCritical("Missing configuration for DDB AccountTable. The AppDDBAccountTable configuration must be set to a DDB AccountTable.");
                throw new Exception("Missing configuration for DDB AccountTable. The AppDDBAccountTable configuration must be set to a DDB AccountTable.");
            }
            else
            {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(Account)] = new Amazon.Util.TypeMapping(typeof(Account), AccountTableName);
            }

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            DDBContext = new DynamoDBContext(ddbClient, config);

            logger.LogInformation($"Configured to use the table: {AccountTableName}");
        }

        [HttpGet]
        [Route("listaccounts")]
        [ProducesResponseType(typeof(List<Account>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> GetAccounts()
        {
            try
            {
                Logger.LogDebug("Getting the Accounts");
                var search = DDBContext.ScanAsync<Account>(null);
                var page = await search.GetNextSetAsync();
                Logger.LogDebug($"Found {page.Count} Account");
                return Ok(page); ;
            }
            catch (Exception ex)
            {
                Logger.LogError("List Accounts Error", ex.Message);
                return BadRequest();
            }
        }

    }
}
