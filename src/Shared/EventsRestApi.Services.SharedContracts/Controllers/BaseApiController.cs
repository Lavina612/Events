using EventsRestApi.Services.SharedContracts.Exceptions;
using EventsRestApi.Services.SharedContracts.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace EventsRestApi.Services.SharedContracts.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        protected Guid CurrentUserId
        {
            get
            {
                var claimValue = User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;

                if (string.IsNullOrEmpty(claimValue) || !Guid.TryParse(claimValue, out var userId))
                {
                    throw new InvalidTokenException(JwtRegisteredClaimNames.NameId);
                }

                return userId;
            }
        }

        protected UserRole CurrentUserRole
        {
            get
            {
                var claimName = "role";

                var claimValue = User.FindFirst(claimName)?.Value;

                if (string.IsNullOrEmpty(claimValue) || !Enum.TryParse<UserRole>(claimValue, true, out var userRole))
                {
                    throw new InvalidTokenException(claimName);
                }

                return userRole;
            }
        }
    }
}
