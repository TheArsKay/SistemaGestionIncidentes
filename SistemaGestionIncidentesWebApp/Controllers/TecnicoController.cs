using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using SistemaGestionIncidentesWebApp.Models;

namespace SistemaGestionIncidentesWebApp.Controllers
{
    public class TecnicoController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;
        public TecnicoController(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpFactory.CreateClient("Api");
            var resp = await client.GetAsync("tecnico/listar");
            if (!resp.IsSuccessStatusCode) return View(new List<Tecnico>());

            var json = await resp.Content.ReadAsStringAsync();
            var lista = JsonSerializer.Deserialize<List<Tecnico>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(lista);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Tecnico model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpFactory.CreateClient("Api");
            var content = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("tecnico/registrar", content);

            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Error al registrar técnico");
                return View(model);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(int id)
        {
            var client = _httpFactory.CreateClient("Api");
            var resp = await client.GetAsync($"tecnico/{id}");
            if (!resp.IsSuccessStatusCode) return NotFound();

            var json = await resp.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<Tecnico>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(int id, Tecnico model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpFactory.CreateClient("Api");
            var content = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PutAsync($"tecnico/actualizar/{id}", content);

            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Error al actualizar técnico");
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var client = _httpFactory.CreateClient("Api");
            var resp = await client.DeleteAsync($"tecnico/eliminar/{id}");
            return RedirectToAction("Index");
        }
    }
}
