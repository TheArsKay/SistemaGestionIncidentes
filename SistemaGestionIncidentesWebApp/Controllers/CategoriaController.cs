using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using SistemaGestionIncidentesWebApp.Models;

namespace SistemaGestionIncidentesWebApp.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;
        public CategoriaController(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpFactory.CreateClient("Api");
            var resp = await client.GetAsync("categoria/listar");
            if (!resp.IsSuccessStatusCode) return View(new List<Categoria>());

            var json = await resp.Content.ReadAsStringAsync();
            var lista = JsonSerializer.Deserialize<List<Categoria>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(lista);
        }

        public async Task<IActionResult> Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Categoria model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpFactory.CreateClient("Api");
            var content = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("categoria/registrar", content);

            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Error al registrar categoría");
                return View(model);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(int id)
        {
            var client = _httpFactory.CreateClient("Api");
            var resp = await client.GetAsync($"categoria/{id}");
            if (!resp.IsSuccessStatusCode) return NotFound();

            var json = await resp.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<Categoria>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(int id, Categoria model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpFactory.CreateClient("Api");
            var content = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PutAsync($"categoria/actualizar/{id}", content);

            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Error al actualizar categoría");
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var client = _httpFactory.CreateClient("Api");
            var resp = await client.DeleteAsync($"categoria/eliminar/{id}");
            // Opcional: comprobar status
            return RedirectToAction("Index");
        }
    }
}