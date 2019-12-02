# ASP.NET Core Web API Serverless Application for User Management using Lambda, API Gateway and DynamoDB

This project shows user management through a serverless approach with lambda acting as a webapi core object. The api utilizes the NuGet package [Amazon.Lambda.AspNetCoreServer](https://www.nuget.org/packages/Amazon.Lambda.AspNetCoreServer) which contains the logic through which a Lambda function that is used to translate requests from API Gateway into the ASP.NET Core framework and then the responses from ASP.NET Core back to API Gateway.

The project has two Web API controllers one for user management and the other for account management. For purpose of ease, both controllers have been configured to be used through base path of the api. The Controllers recieve requests from client and translate them to NoSQL DB queries and commands, in this case DynamoDB. I have kept things simple to one level of abstraction and intentionaly not chosen to go with CQRS approach or using a service bus as it would have complicated the solution further. The idea was to showcase how things can be done in serverless way without the need of provisioning any servers/containers and utilize the pay as you go model. 

An alternate way to achieve this would have been to go with a docker image using docker images to host the solution. That is also a viable solution. 

The project is made as a serverless application which creates a SAM template, which ultimately creates a cloudformation stack creating all the dependencies which are needed for running this solution.

The Application flow looks like below:

API Gateway (WebAPI) ---> Lambda (Business Logic)---> DynamoDB (Data Store)

Following are some of the steps you would need to follow in order to download and run this project.
### Configuring AWS SDK for .NET ###

To integrate the AWS SDK for .NET with the dependency injection system built into ASP.NET Core the NuGet package [AWSSDK.Extensions.NETCore.Setup](https://www.nuget.org/packages/AWSSDK.Extensions.NETCore.Setup/) is referenced. In the Startup.cs file the Amazon DynamoDB client is added to the dependency injection framework. The UserService and AccountService will get their DynamoDB service client from there.

```csharp
 public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    // Add Dynamo to the ASP.NET Core dependency injection framework.
    services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();
    services.AddTransient<IUserService, UserService>();
    services.AddTransient<IAccountService, AccountService>();
}
```

### Configuring for Application Load Balancer ###

You can  configure this project to handle requests from an Application Load Balancer instead of API Gateway. For this your would need to change
the base class of `LambdaEntryPoint` from `Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction` to 
`Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction`.

### Key Serverless Project Files ###

Following are some of the key files to get the application to run

* serverless.template - an AWS CloudFormation Serverless Application Model template file for declaring your Serverless functions and other AWS resources
* aws-lambda-tools-defaults.json - default argument settings for use with Visual Studio and command line deployment tools for AWS
* LambdaEntryPoint.cs - class that derives from **Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction**. The code in 
this file bootstraps the ASP.NET Core hosting framework. The Lambda function is defined in the base class.
Change the base class to **Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction** when using an 
Application Load Balancer.
* LocalEntryPoint.cs - for local development this contains the executable Main function which bootstraps the ASP.NET Core hosting framework with Kestrel, as for typical ASP.NET Core applications.


## Deployment ##

You can choose to deploy this project either via Visual Studio or through command line. For both cases you would need aws configured in your machine in the machine. For intalling aws cli and configuring it on your machine, please refere to : https://docs.aws.amazon.com/cli/latest/userguide/install-cliv1.html

### Deploy from Visual Studio 2017/2019 ###

To deploy this Serverless application:

* Right click the project "Serverless-zip" in Solution Explorer and select *Publish to AWS Lambda*. 
* Select the stack name and S3 bucket to put the lambda package into. The S3 bucket should be in the same region to where you would want to deploy the application.
* On clicking "Next", you would be shown the Template parameters needed to deploy this solution, you can choose to change these values or use the default values, I have populated there.
* Click "Publish", this will deploy your application on the AWS account


### Deploy from the command line: ###

Once you have downloaded and built the code you can deploy your application using the [Amazon.Lambda.Tools Global Tool](https://github.com/aws/aws-extensions-for-dotnet-cli#aws-lambda-amazonlambdatools) from the command line.

Install Amazon.Lambda.Tools Global Tools if not already installed.
```
    dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.
```
    dotnet tool update -g Amazon.Lambda.Tools
```

Execute unit tests
```
    cd "ServerLess-Zip/test/ServerLess-Zip.Tests"
    dotnet test
```

Deploy application
```
    cd "ServerLess-Zip/src/ServerLess-Zip"
    dotnet lambda deploy-serverless
```

## The Endpoints ##
Following are the curl commands for testing each end points exposed by the API.

### List Users ###
```
curl -X GET https://${ServerlessRestApi}.execute-api.${AWS::Region}.amazonaws.com/Prod/listusers
eg : curl -X GET https://ycx5xkpkqa.execute-api.ap-southeast-2.amazonaws.com/Prod/listusers
```

### Create User ###
```
curl -X POST \
  https://${ServerlessRestApi}.execute-api.${AWS::Region}.amazonaws.com/Prod/createuser \
  -H 'Content-Type: application/json' \
  -H 'cache-control: no-cache' \
  -d '{
  "Name": "{userName}",
  "EmailAddress": "{userEmail}",
  "MonthlySalary": {SalaryInDigits},
  "MonthlyExpenses": {MonthlyExpensesInDigits}
}'

eg : curl -X POST \
  https://ycx5xkpkqa.execute-api.ap-southeast-2.amazonaws.com/Prod/createuser \
  -H 'Content-Type: application/json' \
  -H 'cache-control: no-cache' \
  -d '{
  "Name": "kush_test",
  "EmailAddress": "testkush@kushagra.ga",
  "MonthlySalary": 20000,
  "MonthlyExpenses": 6000.50
}'
```

### Get User by Email ###
```
curl -X GET \
  https://${ServerlessRestApi}.execute-api.${AWS::Region}.amazonaws.com/Prod/getuser/{userEmail}\
  -H 'cache-control: no-cache'

eg : curl -X GET \
  https://ycx5xkpkqa.execute-api.ap-southeast-2.amazonaws.com/Prod/getuser/tiwari.kushagra@gmail.com \
  -H 'cache-control: no-cache'
```

### List Accounts ###
```
curl -X GET \
  https://${ServerlessRestApi}.execute-api.${AWS::Region}.amazonaws.com/Prod/listaccounts \
  -H 'cache-control: no-cache'
  
eg : curl -X GET \
  https://ycx5xkpkqa.execute-api.ap-southeast-2.amazonaws.com/Prod/listaccounts \
  -H 'cache-control: no-cache'
```

### Create Account ###
```
curl -X POST \
  https://${ServerlessRestApi}.execute-api.${AWS::Region}.amazonaws.com/Prod/createaccount \
  -H 'Content-Type: application/json' \
  -H 'cache-control: no-cache' \
  -d '{
  "EmailAddress": "{emailAddress}",
  "CreditRequested": {CreditRequestInDigits}
}'

eg : curl -X POST \
  https://ycx5xkpkqa.execute-api.ap-southeast-2.amazonaws.com/Prod/createaccount \
  -H 'Content-Type: application/json' \
  -H 'cache-control: no-cache' \
  -d '{
  "EmailAddress": "test@kushagraa.ga",
  "CreditRequested": 55.64
}'
```

### Get Account by Email ###
```
curl -X GET \
  https://${ServerlessRestApi}.execute-api.${AWS::Region}.amazonaws.com/Prod/getaccount/{userEmail} \
  -H 'cache-control: no-cache'

eg : curl -X GET \
  https://ycx5xkpkqa.execute-api.ap-southeast-2.amazonaws.com/Prod/getaccount/test@kushagra.ga \
  -H 'cache-control: no-cache'
```