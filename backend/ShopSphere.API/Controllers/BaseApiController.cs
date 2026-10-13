using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ShopSphere.API.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User is not authenticated.");

            return int.Parse(userIdClaim);
        }
    }
}