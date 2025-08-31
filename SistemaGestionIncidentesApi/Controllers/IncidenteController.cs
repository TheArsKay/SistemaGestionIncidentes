
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionIncidentesApi.Data.Contrato;
using SistemaGestionIncidentesApi.Models;
//PROBAMOS EL CRUD MEDIANTE EL URL YA SEA POR  POSTMAN Y EL SWAGER
namespace FinancieraAPI.Controllers
{
    // api/Incidente
    [Route("api/incidentes")]
    [ApiController]
    public class IncidenteController : ControllerBase
    {
        private readonly Iincidente incidenteDB;
        public IncidenteController(Iincidente incidenteRepo)
        {
            incidenteDB = incidenteRepo;
        }

        [HttpGet("listarResumen")]
        public ActionResult<IEnumerable<IncidenteListado>> ListarResumen()
        {
            var datos = incidenteDB.ListarIncidentes();
            return Ok(datos);
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            return Ok(await Task.Run(() => incidenteDB.Listado()));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            return Ok(await Task.Run(() => incidenteDB.ObtenerPorID(id)));
        }

        [HttpPost]
        public async Task<IActionResult> Registrar(Incidente incidente)
        {
            return Ok(await Task.Run(() => incidenteDB.Registrar(incidente)));
        }

        [HttpPut]
        public async Task<IActionResult> Actualizar(Incidente incidente)
        {
            return Ok(await Task.Run(() => incidenteDB.Actualizar(incidente)));
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            return Ok(await Task.Run(() => incidenteDB.Eliminar(id)));
        }

    }
}