
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionIncidentesApi.Data.Contrato;
using SistemaGestionIncidentesApi.Models;
using System;
using System.Linq;


namespace SistemaGestionIncidentesApi.Controllers
{

    [Route("api/incidentes")]
    [ApiController]
    public class IncidenteController : ControllerBase
    {
        private readonly Iincidente _incidenteRepo;
        private readonly IEstadoIncidente _estadoRepo;
        private readonly INotificacionRepositorio _notificacionRepo;

        public IncidenteController(Iincidente incidenteRepo, IEstadoIncidente estadoRepo, INotificacionRepositorio notificacionRepo)
        {
            _incidenteRepo = incidenteRepo;
            _estadoRepo = estadoRepo;
            _notificacionRepo = notificacionRepo;
        }

        [HttpGet("listarResumen")]
        public IActionResult ListarResumen()
        {
            var datos = _incidenteRepo.ListarIncidentes();
            return Ok(datos);
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var datos = _incidenteRepo.ListarTodosIncidentes();
            return Ok(datos);
        }


        [HttpGet("{id}")]
        public IActionResult ObtenerPorId(int id)
        {
            var inc = _incidenteRepo.ObtenerPorID(id);
            if (inc == null) return NotFound();
            return Ok(inc);
        }


        [HttpGet("tecnico/{tecnicoId}")]
        public IActionResult ListarPorTecnico(int tecnicoId)
        {
 
            var datos = _incidenteRepo.ListarPorTecnico(tecnicoId);
            return Ok(datos);
        }



        [HttpPost]
        public IActionResult Registrar(Incidente incidente)
        {
            var res = _incidenteRepo.Registrar(incidente);
            return Ok(res);
        }

        [HttpPut("{id}")]
        public IActionResult Actualizar(int id, [FromBody] Incidente incoming)
        {
            var actual = _incidenteRepo.ObtenerPorID(id);
            if (actual == null) return NotFound("Incidente no encontrado.");

     
            actual.TituloIncidente = incoming.TituloIncidente ?? actual.TituloIncidente;
            actual.DescripcionIncidente = incoming.DescripcionIncidente ?? actual.DescripcionIncidente;
            actual.SolucionIncidente = incoming.SolucionIncidente ?? actual.SolucionIncidente;
            actual.idCategoria = incoming.idCategoria != 0 ? incoming.idCategoria : actual.idCategoria;
            actual.idUsuarioTecnico = incoming.idUsuarioTecnico != 0 ? incoming.idUsuarioTecnico : actual.idUsuarioTecnico;
            actual.idUsuarioReporta = incoming.idUsuarioReporta != 0 ? incoming.idUsuarioReporta : actual.idUsuarioReporta;


            if (incoming.idEstadoIncidente != 0)
        {
    
                var estados = _estadoRepo.Listado();


                actual.idEstadoIncidente = incoming.idEstadoIncidente;
        }



         
            actual.FechaModificacion = DateTime.Now; 

            var actualizado = _incidenteRepo.Actualizar(actual);

            try
            {
                if (actualizado != null)
                {
                    var estados = _estadoRepo.Listado();
                    var nombreEstado = estados.FirstOrDefault(e => e.Id == actualizado.idEstadoIncidente)?.NombreEstado ?? "Actualizado";
                    var mensaje = $"Su incidente #{actualizado.Id} ahora está en estado '{nombreEstado}'";
                    _notificacionRepo?.CrearNotificacion(actualizado.idUsuarioReporta, "Estado incidente actualizado", mensaje);
                }
            }
            catch
        {
            
            }

            return Ok(actualizado);
        }



        [HttpDelete("{id}")]
        public IActionResult Eliminar(int id)
        {
            var ok = _incidenteRepo.Eliminar(id);
            if (!ok) return NotFound();
            return Ok(true);
        }
    }
}
