using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SistemaGestionIncidentesWebApp.Models;
using SistemaGestionIncidentesWebApp.Models.DTOs;
using System.Text;

namespace SistemaGestionIncidentesWebApp.Controllers
{
    public class TecnicoController : Controller
    {
        private readonly IConfiguration _config;
        public TecnicoController(IConfiguration config)
        {
            _config = config;
        }

        #region Métodos Privados (Consumo API)
        private List<Tecnico> obtenerTecnicos()
        {
            var lista = new List<Tecnico>();
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var resp = http.GetAsync("tecnico/listar").Result;

                if (!resp.IsSuccessStatusCode)
                    throw new Exception("Error al consumir API de técnicos.");

                var data = resp.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<Tecnico>>(data);
            }
            return lista;
        }

        private Tecnico obtenerPorId(int id)
        {
            Tecnico tecnico = null;
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var resp = http.GetAsync($"tecnico/{id}").Result;

                if (!resp.IsSuccessStatusCode)
                    throw new Exception("Error al obtener técnico por ID.");

                var data = resp.Content.ReadAsStringAsync().Result;
                tecnico = JsonConvert.DeserializeObject<Tecnico>(data);
            }
            return tecnico;
        }

        private Tecnico registrarTecnico(Tecnico tecnico)
        {
            Tecnico nuevo = null;
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                StringContent content = new StringContent(
                    JsonConvert.SerializeObject(tecnico), Encoding.UTF8, "application/json");

                var resp = http.PostAsync("tecnico/registrar", content).Result;

                if (!resp.IsSuccessStatusCode)
                    throw new Exception("Error al registrar técnico.");

                var data = resp.Content.ReadAsStringAsync().Result;
                nuevo = JsonConvert.DeserializeObject<Tecnico>(data);
            }
            return nuevo;
        }

        private Tecnico actualizarTecnico(int id, Tecnico tecnico)
        {
            Tecnico actualizado = null;
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                StringContent content = new StringContent(
                    JsonConvert.SerializeObject(tecnico), Encoding.UTF8, "application/json");

                var resp = http.PutAsync($"tecnico/actualizar/{id}", content).Result;

                if (!resp.IsSuccessStatusCode)
                    throw new Exception("Error al actualizar técnico.");

                var data = resp.Content.ReadAsStringAsync().Result;
                actualizado = JsonConvert.DeserializeObject<Tecnico>(data);
            }
            return actualizado;
        }

        private bool eliminarTecnico(int id)
        {
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var resp = http.DeleteAsync($"tecnico/eliminar/{id}").Result;

                if (!resp.IsSuccessStatusCode)
                    return false;

                var data = resp.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<bool>(data);
            }
        }
        #endregion

        #region Acciones MVC
        public IActionResult Index()
        {
            var tecnicos = obtenerTecnicos();
            return View(tecnicos);
        }

        public IActionResult Create()
        {
            return View(new Tecnico());
        }

        [HttpPost]
        public IActionResult Create(Tecnico tecnico)
        {
            if (!ModelState.IsValid) return View(tecnico);

            var nuevo = registrarTecnico(tecnico);
            if (nuevo == null)
            {
                ModelState.AddModelError("", "No se pudo registrar el técnico.");
                return View(tecnico);
            }
            return RedirectToAction("Index");
        }

 

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var eliminado = eliminarTecnico(id);
            if (!eliminado)
            {
                ModelState.AddModelError("", "No se pudo eliminar el técnico.");
            }
            return RedirectToAction("Index");
        }

        public IActionResult MisIncidentes()
        {
            // 1. Leer el Id del usuario técnico desde la sesión
            var idTecnico = HttpContext.Session.GetInt32("UsuarioId");
            var rol = HttpContext.Session.GetString("UsuarioRol");

            if (idTecnico == null)
            {
                // Si no hay sesión → redirige al login
                return RedirectToAction("Login", "Usuarios");
            }

            // 2. Validar que sea un técnico (opcional pero recomendado)
            if (rol != "Técnico")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            // 3. Consumir el API para traer los incidentes
            var lista = new List<IncidenteTecnicoDTO>();
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var resp = http.GetAsync($"Tecnico/mis-incidentes/{idTecnico}").Result;

                if (resp.IsSuccessStatusCode)
                {
                    var data = resp.Content.ReadAsStringAsync().Result;
                    lista = JsonConvert.DeserializeObject<List<IncidenteTecnicoDTO>>(data);
                }
                else
                {
                    ModelState.AddModelError("", "No se pudieron cargar los incidentes asignados.");
                }
            }

            // 4. Retornar a la vista con los datos
            return View(lista);
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var idTecnico = HttpContext.Session.GetInt32("UsuarioId");
            if (idTecnico == null) return RedirectToAction("Login", "Usuarios");

            IncidenteTecnicoDTO incidente = null;
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var resp = http.GetAsync($"tecnico/mis-incidentes/{idTecnico}/{id}").Result;

                if (resp.IsSuccessStatusCode)
                {
                    var data = resp.Content.ReadAsStringAsync().Result;
                    incidente = JsonConvert.DeserializeObject<IncidenteTecnicoDTO>(data);
                }
            }

            return View(incidente);
        }


        [HttpPost]
        public IActionResult Edit(IncidenteTecnicoDTO incidente)
        {
            if (!ModelState.IsValid) return View(incidente);

            // 🔹 Recuperar idTecnico de la sesión
            var idTecnico = HttpContext.Session.GetInt32("UsuarioId");
            if (idTecnico == null) return RedirectToAction("Login", "Usuarios");

            // 🔹 Asegurar que el DTO viaje con el técnico actual
            incidente.IdTecnico = idTecnico.Value;

            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var content = new StringContent(
                    JsonConvert.SerializeObject(incidente),
                    Encoding.UTF8,
                    "application/json"
                );

                // Llamada al API → PUT
                var resp = http.PutAsync($"tecnico/actualizar-incidente/{incidente.Id}", content).Result;

                if (!resp.IsSuccessStatusCode)
                {
                    var errorMsg = resp.Content.ReadAsStringAsync().Result;
                    ModelState.AddModelError("", $"❌ No se pudo actualizar el incidente. Código: {resp.StatusCode} - {errorMsg}");
                    return View(incidente);
                }
            }

            // 🔹 Redirigir al listado
            return RedirectToAction("MisIncidentes", "Tecnico");
        }





        #endregion
    }
}
