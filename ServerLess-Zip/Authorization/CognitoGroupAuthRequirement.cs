using Microsoft.AspNetCore.Authorization;

namespace ServerLess_API.Authorization
{
    public class CognitoGroupAuthRequirement: IAuthorizationRequirement
    {
        public string CognitoGroup { get; private set; }

        public CognitoGroupAuthRequirement(string cognitoGroup) => CognitoGroup = cognitoGroup;
    }
}
