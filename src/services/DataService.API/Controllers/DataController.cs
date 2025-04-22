using DataService.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataService.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class DataController : ControllerBase
    {
        private readonly ILogger<DataController> _logger;

        public DataController(ILogger<DataController> logger)
        {
            _logger = logger;
        }

        // GET : Establish connectivity with Postgres and fetch all object, and return as json list
        [HttpGet]
        public IEnumerable<Person> Get()
        {
            _logger.LogDebug("Request received : {Controller}->{Action}", nameof(DataController), nameof(Get));
            // Connection


            // Read


            // Return data
            return [];
        }
    }
}
