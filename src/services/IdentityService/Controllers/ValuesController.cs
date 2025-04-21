using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ValuesController : ControllerBase
    {
        // Default constructor
        public ValuesController()
        {
        }

        // GET: values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
