using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerLess_Zip.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ServerLess_API.Authorization;

namespace ServerLess_Zip
{
    public class Startup
    {
        public const string DDBUserTableKey = "DDBUserTable";
        public const string DDBAccountTableKey = "DDBAccountTable";
        public const string CognitoUserPoolId = "CognitoUserPoolId";
        public const string CognitoAppId = "CognitoAppId";
        public const string CognitoRegion = "CognitoRegion";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            
            // Add Dynamo to the ASP.NET Core dependency injection framework.
            services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IAccountService, AccountService>();

            //Cognito authorization provisioning 
            // add our Cognito group authorization requirement, specifying CalendarWriter as the group
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build();
                options.AddPolicy("InPowerUserGroup", policy => policy.Requirements.Add(new CognitoGroupAuthRequirement("CalendarWriter")));
            });

            // add a singleton of our cognito authorization handler
            services.AddSingleton<IAuthorizationHandler, CognitoGroupAuthHandler>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.Audience = Configuration[CognitoAppId];
                o.Authority = $"https://cognito-idp.{Configuration[CognitoRegion]}.amazonaws.com/{CognitoUserPoolId}";
                o.RequireHttpsMetadata = false;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            loggerFactory.AddLambdaLogger(Configuration.GetLambdaLoggerOptions());

            app.UseDefaultFiles(); //needs to be before the app.UseStaticFiles() call below

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseMvc();
        }
    }
}
