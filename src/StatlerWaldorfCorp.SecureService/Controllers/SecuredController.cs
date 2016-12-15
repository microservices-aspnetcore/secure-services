using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StatlerWaldorfCorp.Secureservice.Controllers
{
    [Route("/api/secured")]
    public class SecuredController : Controller
    {
        [Authorize]
        [HttpGet]
        public string Get() 
        {
            return "This is from the super secret area";
        }
    }
}