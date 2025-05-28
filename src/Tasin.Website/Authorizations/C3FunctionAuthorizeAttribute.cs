using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Tasin.Website.Common.CommonModels;

namespace Tasin.Website.Authorizations
{
    public class C3FunctionAuthorizationAttribute : AuthorizeAttribute
    {
        public C3FunctionAuthorizationAttribute(bool redirect = false, params int[] functionIdList)
        {
            PermissionId = new()
            {
                ListPermission = functionIdList.ToList(),
                Redirect = redirect
            };
        }

        public Permission PermissionId
        {
            get
            {
                return new Permission();
            }
            set
            {

                Policy = JsonConvert.SerializeObject(value);
            }
        }


    }
}
