
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionIncidentesApi.Data.Contrato;
using SistemaGestionIncidentesApi.Models;

namespace FinancieraAPI.Controllers
{
    // api/Incidente
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly Iincidente incidenteDB;
        public ClientesController(Iincidente clienteRepo)
        {
            incidenteDB = clienteRepo;
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
        public async Task<IActionResult> Registrar(Incidente cliente)
        {
            return Ok(await Task.Run(() => incidenteDB.Registrar(cliente)));
        }
    }
}
