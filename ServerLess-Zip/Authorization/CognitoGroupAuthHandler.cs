using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ServerLess_API.Authorization
{
    public class CognitoGroupAuthHandler:AuthorizationHandler<CognitoGroupAuthRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CognitoGroupAuthRequirement requirement)
        {
            if (context.User.HasClaim(c => c.Type == "cognito:groups" && c.Value == requirement.CognitoGroup))
            {

                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
