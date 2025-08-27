using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using SistemaGestionIncidentesWebApp.Models;

namespace SistemaGestionIncidentesWebApp.Controllers
{
    public class IncidenteController : Controller
    {
        private readonly IConfiguration _config;

        public IncidenteController(IConfiguration config)
        {
            _config = config;
        }

        #region Métodos Privados (Consumo API)

        private List<IncidenteListado> obtenerIncidentesResumen()
        {
            var lista = new List<IncidenteListado>();
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var msg = http.GetAsync("incidentes/listarResumen").Result;

                if (!msg.IsSuccessStatusCode)
                    throw new Exception("Error al consumir el API de incidentes (listarResumen).");

                var data = msg.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<IncidenteListado>>(data);
            }
            return lista;
        }

        private List<Incidente> obtenerIncidentes()
        {
            var lista = new List<Incidente>();
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var msg = http.GetAsync("incidentes").Result;

                if (!msg.IsSuccessStatusCode)
                    throw new Exception("Error al consumir el API de incidentes (listar).");

                var data = msg.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<Incidente>>(data);
            }
            return lista;
        }

        private Incidente obtenerPorId(int id)
        {
            Incidente incidente = null;
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var msg = http.GetAsync($"incidentes/{id}").Result;

                if (!msg.IsSuccessStatusCode)
                    throw new Exception("Error al obtener el incidente por ID.");

                var data = msg.Content.ReadAsStringAsync().Result;
                incidente = JsonConvert.DeserializeObject<Incidente>(data);
            }
            return incidente;
        }

        private Incidente registrarIncidente(Incidente incidente)
        {
            Incidente nuevo = null;
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var msg = http.PostAsJsonAsync("incidentes", incidente).Result;

                if (!msg.IsSuccessStatusCode)
                    throw new Exception("Error al registrar el incidente.");

                var data = msg.Content.ReadAsStringAsync().Result;
                nuevo = JsonConvert.DeserializeObject<Incidente>(data);
            }
            return nuevo;
        }

        private Incidente actualizarIncidente(Incidente incidente)
        {
            Incidente actualizado = null;
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var msg = http.PutAsJsonAsync("incidentes", incidente).Result;

                if (!msg.IsSuccessStatusCode)
                    throw new Exception("Error al actualizar el incidente.");

                var data = msg.Content.ReadAsStringAsync().Result;
                actualizado = JsonConvert.DeserializeObject<Incidente>(data);
            }
            return actualizado;
        }

        private bool eliminarIncidente(int id)
        {
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var msg = http.DeleteAsync($"incidentes/{id}").Result;

                if (!msg.IsSuccessStatusCode)
                    throw new Exception("Error al eliminar el incidente.");

                var data = msg.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<bool>(data);
            }
        }


        private List<Usuario> obtenerUsuarios()
        {
            var lista = new List<Usuario>();
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var msg = http.GetAsync("usuarios").Result;
                var data = msg.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<Usuario>>(data);
            }
            return lista;
        }

        private List<Tecnico> obtenerTecnicos()
        {
            var lista = new List<Tecnico>();
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var msg = http.GetAsync("tecnico/listar").Result;
                var data = msg.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<Tecnico>>(data);
            }
            return lista;
        }

        private List<EstadoIncidente> obtenerEstados()
        {
            var lista = new List<EstadoIncidente>();
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var msg = http.GetAsync("estados-incidente").Result;
                var data = msg.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<EstadoIncidente>>(data);
            }
            return lista;
        }

        private List<Categoria> obtenerCategorias()
        {
            var lista = new List<Categoria>();
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var msg = http.GetAsync("categoria/listar").Result;
                var data = msg.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<Categoria>>(data);
            }
            return lista;
        }


        #endregion


        // Listados
        public IActionResult MantenerIncidente(int page = 1, int numreg = 10)
        {
            var incidentes = obtenerIncidentes();

            // === Combos para mostrar nombres en vez de IDs ===
            var usuarios = obtenerUsuarios();
            ViewBag.Usuarios = new SelectList(usuarios, "Id", "Nombre");

            var tecnicos = obtenerTecnicos();
            ViewBag.Tecnicos = new SelectList(tecnicos, "Id", "Nombre");

            var estados = obtenerEstados();
            ViewBag.Estados = new SelectList(estados, "Id", "NombreEstado");

            var categorias = obtenerCategorias();
            ViewBag.Categorias = new SelectList(categorias, "Id", "Nombre");

            // === Paginación simple ===
            int totalRegistros = incidentes.Count();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / numreg);
            int omitir = numreg * (page - 1);

            ViewBag.totalPaginas = totalPaginas;
            ViewBag.page = page;
            ViewBag.numreg = numreg;

            var resultado = incidentes.Skip(omitir).Take(numreg);

            return View(resultado);
        }



        public IActionResult ListadoIncidente(int page = 1, int numreg = 10, int usuario = 0, int tecnico = 0, int estado = 0, int categoria = 0)
        {
            var incidentes = obtenerIncidentes(); // trae lista completa

            // === Filtros por ID ===
            if (usuario > 0)
                incidentes = incidentes.Where(i => i.Id_Usuario == usuario).ToList();
            if (tecnico > 0)
                incidentes = incidentes.Where(i => i.Id_Tecnico == tecnico).ToList();
            if (estado > 0)
                incidentes = incidentes.Where(i => i.Id_Estado == estado).ToList();
            if (categoria > 0)
                incidentes = incidentes.Where(i => i.Id_Categoria == categoria).ToList();

            // === Combos ===
            var usuarios = obtenerUsuarios();
            usuarios.Insert(0, new Usuario { Id = 0, Nombre = "--SELECCIONE--" });
            ViewBag.Usuarios = new SelectList(usuarios, "Id", "Nombre", usuario);

            var tecnicos = obtenerTecnicos();
            tecnicos.Insert(0, new Tecnico { Id = 0, Nombre = "--SELECCIONE--" });
            ViewBag.Tecnicos = new SelectList(tecnicos, "Id", "Nombre", tecnico);

            var estados = obtenerEstados();
            estados.Insert(0, new EstadoIncidente { Id = 0, NombreEstado = "--SELECCIONE--" });
            ViewBag.Estados = new SelectList(estados, "Id", "NombreEstado", estado);

            var categorias = obtenerCategorias();
            categorias.Insert(0, new Categoria { Id = 0, Nombre = "--SELECCIONE--" });
            ViewBag.Categorias = new SelectList(categorias, "Id", "Nombre", categoria);

            // Guardar valores seleccionados (para paginación)
            ViewBag.usuarioSeleccionado = usuario;
            ViewBag.tecnicoSeleccionado = tecnico;
            ViewBag.estadoSeleccionado = estado;
            ViewBag.categoriaSeleccionado = categoria;

            // === Paginación ===
            int totalRegistros = incidentes.Count();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / numreg);
            int omitir = numreg * (page - 1);

            ViewBag.totalPaginas = totalPaginas;
            ViewBag.page = page;
            ViewBag.numreg = numreg;

            var resultado = incidentes.Skip(omitir).Take(numreg);

            return View(resultado);
        }


        // Crear
        public IActionResult Create()
        {
            return View(new Incidente()); // modelo vacío
        }

        [HttpPost]
        public IActionResult Create(Incidente incidente)
        {
            var nuevo = registrarIncidente(incidente);
            return RedirectToAction("MantenerIncidente");
        }

        // Editar
        public IActionResult Edit(int id)
        {
            var incidente = obtenerPorId(id);
            return View(incidente);
        }

        [HttpPost]
        public IActionResult Edit(Incidente incidente)
        {
            var actualizado = actualizarIncidente(incidente);
            return RedirectToAction("MantenerIncidente");
        }

        // Eliminar
        public IActionResult Delete(int id)
        {
            var incidente = obtenerPorId(id);
            return View(incidente);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var eliminado = eliminarIncidente(id);
            return RedirectToAction("MantenerIncidente");
        }

      
    }
}
