using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerLess_Zip.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerLess_Zip.Services
{
    public class AccountService : IAccountService
    {
        private IAmazonDynamoDB DDBClient { get; set; }
        private ILogger Logger { get; set; }
        private IDynamoDBContext DDBContext { get; set; }
        private string AccountTableName { get; set; }

        public AccountService(IConfiguration configuration, ILogger<AccountService> logger, IAmazonDynamoDB ddbClient)
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

        public Task<Account> GetAccountByEmail(string email)
        {
            Logger.LogInformation($"Getting account details with {email}.");

            var account = DDBContext.LoadAsync<Account>(email.ToLower());

            Logger.LogInformation($"Found account: {account != null}");

            return account;
        }

        /// <summary>
        /// Gets a list of all accounts with their details
        /// </summary>
        /// <returns></returns>
        public Task<List<Account>> GetAllAccounts()
        {
            Logger.LogDebug("Getting the Accounts");
            var search = DDBContext.ScanAsync<Account>(null);
            return search.GetNextSetAsync();
        }

        /// <summary>
        /// Creates account for the user, this doesnt include any validation.
        /// </summary>
        /// <param name="accountRequest"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task CreateAccount(AccountRequest accountRequest, User user)
        {
            Logger.LogInformation($"Creation of account for user: {user.EmailAddress} started.");

            var account = new Account
            {
                EmailAddress = accountRequest.EmailAddress,
                CreditTaken = accountRequest.CreditRequested,
                AvailableSurplus = user.MonthlySalary - user.MonthlyExpenses -accountRequest.CreditRequested
            };

            Logger.LogInformation($"Saving account for: {user.EmailAddress}");

            return DDBContext.SaveAsync<Account>(account);
        }


    }
}
