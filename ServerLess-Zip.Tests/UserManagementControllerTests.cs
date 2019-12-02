using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace ServerLess_Zip.Tests
{
    public class UserManagementControllerTests 
    {
        string UserTableName { get; set; }
        IAmazonDynamoDB DDBClient { get; set; }

        IConfigurationRoot Configuration { get; set; }


        public UserManagementControllerTests()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            this.Configuration = builder.Build();

            // Use the region and possible profile specified in the appsettings.json file to construct an Amaozn S3 service client.
            this.DDBClient = Configuration.GetAWSOptions().CreateServiceClient<IAmazonDynamoDB>();

            // Create a UserTable used for the test which will be deleted along with any data in the UserTable once the test is complete.
            this.UserTableName = "lambda-UserManagementControllerTests-".ToLower() + DateTime.Now.Ticks;
            this.DDBClient.CreateTableAsync(
                this.UserTableName,
                new List<KeySchemaElement>
                {
                    new KeySchemaElement { KeyType = KeyType.HASH, AttributeName = "EmailAddress" }
                },
                new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = "EmailAddress", AttributeType = ScalarAttributeType.S }
                },
                new ProvisionedThroughput { ReadCapacityUnits = 3, WriteCapacityUnits = 3 }).Wait();
        }
    }
}
