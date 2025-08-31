using Microsoft.AspNetCore.Mvc;
using SistemaGestionIncidentesApi.Data.Contrato;
using SistemaGestionIncidentesApi.Models;
using SistemaGestionIncidentesApi.Models.DTOs;
using System.Threading.Tasks;

namespace SistemaGestionIncidentesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TecnicoController : ControllerBase
    {
        private readonly ITecnico tecnicoDB;

        public TecnicoController(ITecnico tecnicoRepo)
        {
            tecnicoDB = tecnicoRepo;
        }

        [HttpGet("listar")]
        public async Task<IActionResult> Listar()
        {
            return Ok(await Task.Run(() => tecnicoDB.Listar()));
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] Tecnico tecnico)
        {
            return Ok(await Task.Run(() => tecnicoDB.Registrar(tecnico)));
        }

        [HttpPut("actualizar/{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Tecnico tecnico)
        {
            tecnico.Id = id;
            return Ok(await Task.Run(() => tecnicoDB.Actualizar(tecnico)));
        }

        [HttpDelete("eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            return Ok(await Task.Run(() => tecnicoDB.Eliminar(id)));
        }

        // 🔹 Recuperar los incidentes asignados al técnico
        [HttpGet("mis-incidentes/{idTecnico}")]
        public async Task<IActionResult> MisIncidentes(int idTecnico)
        {
            return Ok(await Task.Run(() => tecnicoDB.ListarIncidentesPorTecnico(idTecnico)));
        }

        [HttpPut("actualizar-incidente/{id}")]
        public async Task<IActionResult> ActualizarIncidentePorTecnico(int id, [FromBody] IncidenteTecnicoDTO dto)
        {
            // Validar que venga el idTecnico
            if (dto.IdTecnico <= 0)
                return BadRequest("El técnico no está identificado.");

            // Antes de actualizar, comprobar que el incidente pertenece a ese técnico
            var incidente = await Task.Run(() =>
                tecnicoDB.ObtenerIncidentePorTecnico(dto.IdTecnico, id)
            );

            if (incidente == null)
                return Unauthorized("El incidente no pertenece a este técnico o no existe.");

            // Ejecutar la actualización
            var exito = await Task.Run(() =>
                tecnicoDB.ActualizarIncidentePorTecnico(id, dto.IdEstado, dto.SolucionIncidente)
            );

            if (!exito)
                return BadRequest("No se pudo actualizar el incidente.");

            return Ok("Incidente actualizado correctamente.");
        }

        [HttpGet("mis-incidentes/{idTecnico}/{idIncidente}")]
        public async Task<IActionResult> ObtenerIncidentePorTecnico(int idTecnico, int idIncidente)
        {
            return Ok(await Task.Run(() =>
                tecnicoDB.ObtenerIncidentePorTecnico(idTecnico, idIncidente)
            ));
        }


    }
}
