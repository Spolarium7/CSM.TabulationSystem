using CSM.TabulationSystem.Web.Infrastructure.Data.Enums;
using CSM.TabulationSystem.Web.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Infrastructure.Authentication
{
    public class AuthorizeAdminRequirementHandler : AuthorizationHandler<AuthorizeAdminRequirement>
    {
        public AuthorizeAdminRequirementHandler() { }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizeAdminRequirement requirement)
        {
            if (WebUser.SystemRole != SystemRole.Admin)
            {
                context.Fail();

                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

    public class AuthorizeAdminRequirement : IAuthorizationRequirement
    {
    }
}
