using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SistemaGestionIncidentesWebApp.Models;

namespace SistemaGestionIncidentesWebApp.Controllers
{
    public class IncidentesController : Controller
    {
        private readonly IConfiguration _config;

        public IncidentesController(IConfiguration config)
        {
            _config = config;
        }

        private List<IncidenteListado> obtenerIncidentes()
        {
            var lista = new List<IncidenteListado>();

            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var msg = http.GetAsync("Incidentes/listar").Result;

                if (!msg.IsSuccessStatusCode)
                {
                    throw new Exception("Error al consumir el API de incidentes.");
                }

                var data = msg.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<IncidenteListado>>(data);
            }

            return lista;
        }


        public IActionResult Index()
        {
            var listado = obtenerIncidentes();
            return View(listado);
        }
    }

}

