using EventsRestApi.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace EventsRestApi.Presentation.Controllers
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
    }
}
