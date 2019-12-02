using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerLess_Zip.Model;

namespace ServerLess_Zip.Services
{
    public class UserService : IUserService
    {
        private IAmazonDynamoDB DDBClient { get; set; }
        private ILogger Logger { get; set; }
        private IDynamoDBContext DDBContext { get; set; }
        private string UserTableName { get; set; }

        public UserService(IConfiguration configuration, ILogger<UserService> logger, IAmazonDynamoDB ddbClient)
        {
            Logger = logger;
            DDBClient = ddbClient;

            UserTableName = configuration[Startup.AppDDBTableKey];
            if (string.IsNullOrEmpty(UserTableName))
            {
                var errorMessage = "Missing configuration for DDB UserTable. The AppDDBTable configuration must be set to a DDB UserTable.";
                Logger.LogCritical(errorMessage);
                throw new Exception(errorMessage);
            }
            else
            {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(User)] = new Amazon.Util.TypeMapping(typeof(User), UserTableName);
            }

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            DDBContext = new DynamoDBContext(ddbClient, config);

            logger.LogInformation($"Configured to use the table: {UserTableName}");
        }

        /// <summary>
        /// Gets the user from DDB using partition key email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task<User> GetUserByEmail(string email)
        {
            Logger.LogInformation($"Getting user details with {email}.");

            var user = DDBContext.LoadAsync<User>(email.ToLower());

            Logger.LogInformation($"Found user: {user != null}");

            return user;
        }


        /// <summary>
        /// Gets the list of users from DDB
        /// </summary>
        /// <returns></returns>
        public Task<List<User>> GetAllUsers()
        {
            Logger.LogDebug("Getting the users");
            var search = DDBContext.ScanAsync<User>(null);
            var page =  search.GetNextSetAsync();
            return page;
        }

        /// <summary>
        /// Method to create users. This method would not do any validations. Validations are segregated at controller level
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task CreateUser(User user)
        {
            Logger.LogInformation($"Creation for User: {user.EmailAddress} started.");
            Logger.LogInformation($"Saving User: {user.EmailAddress}");

            return DDBContext.SaveAsync<User>(user);
        }
    }
}
