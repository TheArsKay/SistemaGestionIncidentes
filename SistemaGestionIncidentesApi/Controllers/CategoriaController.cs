using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using SistemaGestionIncidentesApi.Models;

namespace SistemaGestionIncidentesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly IConfiguration _config;
        public CategoriaController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("listar")]
        public IActionResult Listar()
        {
            var lista = new List<Categoria>();
            var connStr = _config.GetConnectionString("DB");

            using (SqlConnection cn = new SqlConnection(connStr))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand("ListarCategorias", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Categoria
                            {
                                Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                Nombre = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                            });
                        }
                    }
                }
            }

            return Ok(lista);
        }

        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] Categoria model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Nombre))
                return BadRequest("Nombre inválido");

            var connStr = _config.GetConnectionString("DB");
            using (SqlConnection cn = new SqlConnection(connStr))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand("RegistrarCategoria", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nombre_categoria", model.Nombre);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok(new { success = true });
        }

        [HttpPut("actualizar/{id}")]
        public IActionResult Actualizar(int id, [FromBody] Categoria model)
        {
            if (model == null || id <= 0) return BadRequest();

            var connStr = _config.GetConnectionString("DB");
            using (SqlConnection cn = new SqlConnection(connStr))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand("ActualizarCategoria", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@nombre_categoria", model.Nombre);
                    cmd.Parameters.AddWithValue("@estado", model.Estado);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok(new { success = true });
        }

        [HttpDelete("eliminar/{id}")]
        public IActionResult Eliminar(int id)
        {
            if (id <= 0) return BadRequest();

            var connStr = _config.GetConnectionString("DB");
            using (SqlConnection cn = new SqlConnection(connStr))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand("EliminarCategoria", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok(new { success = true });
        }
    }
}
