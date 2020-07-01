using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Serialization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Collections.Generic;
using Web.Api.Core.Domain;
using Web.Api.Models.Response;
using Newtonsoft.Json;
using System;

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
            using (var _httpClient = _httpClientFactory.CreateClient("GitHub"))
            {
              //_httpClient.DefaultRequestHeaders.Accept.Clear();
              //_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
              var response = _httpClient.GetAsync("http://192.168.99.100:5200/api/demanda").Result;

              response.EnsureSuccessStatusCode();

              var conteudo = response.Content.ReadAsStringAsync().Result;
              List<Demanda> resultado = JsonConvert.DeserializeObject<List<Demanda>>(conteudo);
              return Ok(resultado);
            }
        }      
    }
}
