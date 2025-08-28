
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionIncidentesApi.Data.Contrato;
using SistemaGestionIncidentesApi.Models;

namespace SistemaGestionIncidentesApi.Controllers
{
 
    [Route("api/estados-incidente")]
    [ApiController]
    public class EstadoIncidenteController : ControllerBase
    {
        private readonly IEstadoIncidente estadoIncidenteDB;
        public EstadoIncidenteController(IEstadoIncidente estadoIncidenteRepo)
        {
            estadoIncidenteDB = estadoIncidenteRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            return Ok(await Task.Run(() => estadoIncidenteDB.Listado()));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            return Ok(await Task.Run(() => estadoIncidenteDB.ObtenerPorID(id)));
        }

        [HttpPost]
        public async Task<IActionResult> Registrar(EstadoIncidente estadoIncidente)
        {
            return Ok(await Task.Run(() => estadoIncidenteDB.Registrar(estadoIncidente)));
        }

        [HttpPut]
        public async Task<IActionResult> Actualizar(EstadoIncidente estadoIncidente)
        {
            return Ok(await Task.Run(() => estadoIncidenteDB.Actualizar(estadoIncidente)));
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            return Ok(await Task.Run(() => estadoIncidenteDB.Eliminar(id)));
        }
    }
}
