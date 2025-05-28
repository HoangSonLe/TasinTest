using Microsoft.AspNetCore.Authorization;
using Tasin.Website.Common.CommonModels;

namespace Tasin.Website.Authorizations
{
    public class C3FunctionAuthorizationRequirement : IAuthorizationRequirement
    {
        public Permission PermissionId { get; private set; }
        public C3FunctionAuthorizationRequirement(Permission permissionId)
        {
            PermissionId = permissionId;
        }
    }
}
