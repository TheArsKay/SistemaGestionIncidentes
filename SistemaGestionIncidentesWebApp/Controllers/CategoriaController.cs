using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SistemaGestionIncidentesWebApp.Models;
using System.Data;

namespace SistemaGestionIncidentesWebApp.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly IConfiguration _configuration;

        public CategoriaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // 📌 Página principal
        public IActionResult Index()
        {
            return View();
        }

        // 📌 Listar categorías en JSON
        [HttpGet]
        public IActionResult Listar()
        {
            List<Categoria> categorias = new List<Categoria>();

            using (SqlConnection cn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("ListarCategorias", cn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    categorias.Add(new Categoria
                    {
                        Id = Convert.ToInt32(dr["id"]),
                        NombreCategoria = dr["nombre_categoria"].ToString(),
                        Estado = dr["estado"].ToString()
                    });
                }
            }

            return Json(categorias);
        }

        // 📌 Crear categoría
        [HttpPost]
        public IActionResult CrearJson([FromBody] Categoria categoria)
        {
            if (categoria == null || string.IsNullOrWhiteSpace(categoria.NombreCategoria))
                return BadRequest("El nombre de la categoría es obligatorio");

            using (SqlConnection cn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("RegistrarCategoria", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nombre_categoria", categoria.NombreCategoria);
                cmd.ExecuteNonQuery();
            }

            return Json(new { success = true });
        }

        // 📌 Editar categoría
        [HttpPost]
        public IActionResult EditarJson([FromBody] Categoria categoria)
        {
            if (categoria == null || categoria.Id <= 0 || string.IsNullOrWhiteSpace(categoria.NombreCategoria))
                return BadRequest("Datos inválidos");

            using (SqlConnection cn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("ActualizarCategoria", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", categoria.Id);
                cmd.Parameters.AddWithValue("@nombre_categoria", categoria.NombreCategoria);
                cmd.Parameters.AddWithValue("@estado", categoria.Estado ?? "A");
                cmd.ExecuteNonQuery();
            }

            return Json(new { success = true });
        }

        // 📌 Eliminar categoría (baja lógica)
        [HttpPost]
        public IActionResult EliminarJson([FromBody] Categoria categoria)
        {
            if (categoria == null || categoria.Id <= 0)
                return BadRequest("Id inválido");

            using (SqlConnection cn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("EliminarCategoria", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", categoria.Id);
                cmd.ExecuteNonQuery();
            }

            return Json(new { success = true });
        }
    }
}
