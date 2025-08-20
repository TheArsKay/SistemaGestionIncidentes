using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionIncidentesApi.Data.Contrato;
using SistemaGestionIncidentesApi.Models;

namespace SistemaGestionIncidentesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuario usuarioDB;
        public UsuariosController(IUsuario usuarioRepo)
        {
            usuarioDB = usuarioRepo;
        }
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            return Ok(await Task.Run(()=> usuarioDB.Listado()));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            return Ok(await Task.Run(() => usuarioDB.ObtenerPorID(id)));
        }

        [HttpPost("iniciar_sesion")]
        public async Task<IActionResult> IniciarSesion(string email, string clave)
        {
            return Ok(await Task.Run(() => usuarioDB.IniciarSesion(email, clave)));
        }

        [HttpPost]
        [Route("registrar")]
        public async Task<IActionResult> RegistrarUsuario(Usuario usuario)
        {
            return Ok(await Task.Run(() => usuarioDB.RegistrarUsuario(usuario)));
        }

        [HttpGet("roles")]
        public IActionResult Roles()
        {
            return Ok(usuarioDB.ListarRoles());
        }

    }
}
