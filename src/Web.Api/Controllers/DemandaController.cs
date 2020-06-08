using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Serialization;
using System.Net;
using System.Net.Http;
using System.Linq;
using Web.Api.Core.Domain;

namespace Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemandaController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DemandaController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> GetDemandas()
        {
            var client = _httpClientFactory.CreateClient("GitHub"); 
            return Ok(await client.GetStringAsync("/someapi"));
        }      
    }
}
