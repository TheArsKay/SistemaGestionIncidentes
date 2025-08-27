using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SistemaGestionIncidentesApi.Models;
using System.Data;

namespace SistemaGestionIncidentesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TecnicoController : ControllerBase
    {
        private readonly IConfiguration _config;
        public TecnicoController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("listar")]
        public IActionResult Listar()
        {
            var lista = new List<Tecnico>();
            var connStr = _config.GetConnectionString("DB");

            using (SqlConnection cn = new SqlConnection(connStr))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand("ListarTecnicos", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Tecnico
                            {
                                Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                Nombre = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                Correo = reader.FieldCount > 2 && !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty
                            });
                        }
                    }
                }
            }

            return Ok(lista);
        }

        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] Tecnico model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Nombre) || string.IsNullOrWhiteSpace(model.Clave))
                return BadRequest("Nombre o Clave inválidos");

            var connStr = _config.GetConnectionString("DB");
            using (SqlConnection cn = new SqlConnection(connStr))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand("RegistrarTecnico", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@nombre", model.Nombre);
                    cmd.Parameters.AddWithValue("@clave", model.Clave);
                    cmd.Parameters.AddWithValue("@correo", model.Correo ?? (object)DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }

            return Ok(new { success = true });
        }

        [HttpPut("actualizar/{id}")]
        public IActionResult Actualizar(int id, [FromBody] Tecnico model)
        {
            if (model == null || id <= 0) return BadRequest();

            var connStr = _config.GetConnectionString("DB");
            using (SqlConnection cn = new SqlConnection(connStr))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand("ActualizarTecnico", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@nombre", model.Nombre);
                    cmd.Parameters.AddWithValue("@correo", model.Correo ?? (object)DBNull.Value);
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
                using (SqlCommand cmd = new SqlCommand("EliminarTecnico", cn))
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