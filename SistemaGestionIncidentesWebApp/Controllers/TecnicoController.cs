using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SistemaGestionIncidentesWebApp.Models;
using System.Data;

namespace SistemaGestionIncidentesWebApp.Controllers
{
    public class TecnicoController : Controller
    {
        private readonly IConfiguration _configuration;

        public TecnicoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Listar()
        {
            List<Usuario> tecnicos = new List<Usuario>();

            using (SqlConnection cn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("ListarTecnicos", cn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    tecnicos.Add(new Usuario
                    {
                        Id = Convert.ToInt32(dr["id"]),
                        Nombre = dr["nombre"].ToString(),
                        Email = dr["email"].ToString(),
                        RolId = Convert.ToInt32(dr["rol_id"]),
                        nombreRol = dr["nombreRol"].ToString()
                    });
                }
            }

            return Json(tecnicos);
        }

        [HttpPost]
        public IActionResult Crear([FromBody] Usuario tecnico)
        {
            ModelState.Remove("Id");

            if (tecnico == null || string.IsNullOrWhiteSpace(tecnico.Nombre)
                || string.IsNullOrWhiteSpace(tecnico.Email)
                || string.IsNullOrWhiteSpace(tecnico.Clave))
                return BadRequest("Todos los campos son obligatorios");

            using (SqlConnection cn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("RegistrarTecnico", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nombre", tecnico.Nombre);
                cmd.Parameters.AddWithValue("@clave", tecnico.Clave);
                cmd.Parameters.AddWithValue("@correo", tecnico.Email);
                cmd.ExecuteNonQuery();
            }

            return Ok(new { success = true });
        }

        [HttpPost]
        public JsonResult Editar([FromBody] Usuario tecnico)
        {
            using (SqlConnection cn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("ActualizarTecnico", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", tecnico.Id);
                cmd.Parameters.AddWithValue("@nombre", tecnico.Nombre);
                cmd.Parameters.AddWithValue("@correo", tecnico.Email);
                cmd.Parameters.AddWithValue("@estado", "A");
                cmd.ExecuteNonQuery();
            }
            return Json(new { success = true, message = "Técnico actualizado correctamente" });
        }

        [HttpPost]
        public IActionResult Eliminar([FromBody] IdRequest request)
        {
            if (request == null || request.Id <= 0)
                return BadRequest("Id inválido");

            using (SqlConnection cn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("EliminarTecnico", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", request.Id);
                cmd.ExecuteNonQuery();
            }

            return Ok(new { success = true });
        }

        // DTO simple
        public class IdRequest
        {
            public int Id { get; set; }
        }

    }
}
