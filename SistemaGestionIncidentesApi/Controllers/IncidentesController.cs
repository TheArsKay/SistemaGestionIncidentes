using Microsoft.AspNetCore.Mvc;
using SistemaGestionIncidentesApi.Data.Contrato;
using SistemaGestionIncidentesApi.Models;

namespace SistemaGestionIncidentesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentesController : ControllerBase
    {
        private readonly IIndicente incidenteDB;
        public IncidentesController(IIndicente incidenteRepo)
        {
            incidenteDB = incidenteRepo;
        }

        [HttpGet("listar")]
        public ActionResult<IEnumerable<IncidenteListado>> Listar()
        {
            var datos = incidenteDB.ListarIncidentes();
            return Ok(datos);
        }
    }
}
