using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using SistemaGestionIncidentesWebApp.Models;

namespace DSW1_T2_MANTARI_ALVARADO_MERCEDES_WEB.Controllers
{
    public class IncidenteController : Controller
    {
        private readonly IConfiguration _mbmaConfig;
        public IncidenteController(IConfiguration iConfig)
        {
            _mbmaConfig = iConfig;
        }

        #region . MÉTODOS PRIVADOS .

        private List<Incidente> obtenerIncidente()
        {
            var lstIncidente = new List<Incidente>();
            using (var clienteHTTP = new HttpClient())
            {
                clienteHTTP.BaseAddress = new Uri(_mbmaConfig["Services:URL"]);

                var mensaje = clienteHTTP.GetAsync("incidentes").Result;

                var data = mensaje.Content.ReadAsStringAsync().Result;

                lstIncidente = JsonConvert.DeserializeObject<List<Incidente>>(data);
            }
            return lstIncidente;
        }
        private List<EstadoIncidente> obtenerEstadosIncidente()
        {
            var lstEstadoIncidente = new List<EstadoIncidente>();
            using (var clienteHTTP = new HttpClient())
            {
                clienteHTTP.BaseAddress = new Uri(_mbmaConfig["Services:URL"]);

                var mensaje = clienteHTTP.GetAsync("estados-incidente").Result;

                var data = mensaje.Content.ReadAsStringAsync().Result;

                lstEstadoIncidente = JsonConvert.DeserializeObject<List<EstadoIncidente>>(data);
            }
            return lstEstadoIncidente;
        }

        private Incidente obtenerPorId(int id)
        {
            Incidente incidente = new Incidente();
            using (var clienteHTTP = new HttpClient())
            {
                clienteHTTP.BaseAddress = new Uri(_mbmaConfig["Services:URL"]);
                var mensaje = clienteHTTP.GetAsync($"incidentes/{id}").Result;
                var data = mensaje.Content.ReadAsStringAsync().Result;
                incidente = JsonConvert.DeserializeObject<Incidente>(data);
            }
            return incidente;
        }

        private Incidente registrarIncidente(Incidente mbmaProducto)
        {
            Incidente nuevo = new Incidente();
            using (var clienteHTTP = new HttpClient())
            {
                clienteHTTP.BaseAddress = new Uri(_mbmaConfig["Services:URL"]);

                StringContent contenido = new StringContent(JsonConvert.SerializeObject(mbmaProducto),
                    System.Text.Encoding.UTF8, "application/json");
                var mensaje = clienteHTTP.PostAsync("incidentes", contenido).Result;
                var data = mensaje.Content.ReadAsStringAsync().Result;
                nuevo = JsonConvert.DeserializeObject<Incidente>(data);
            }

            return nuevo;
        }

        private Incidente actualizarIncidente(Incidente incidente)
        {
            using (var clienteHTTP = new HttpClient())
            {
                clienteHTTP.BaseAddress = new Uri(_mbmaConfig["Services:URL"]);
                var contenido = new StringContent(JsonConvert.SerializeObject(incidente),
                    System.Text.Encoding.UTF8, "application/json");
                var mensaje = clienteHTTP.PutAsync("incidentes", contenido).Result;
                var data = mensaje.Content.ReadAsStringAsync().Result;
                incidente = JsonConvert.DeserializeObject<Incidente>(data);
                
            }
            return incidente;
        }

        private bool eliminarProducto(int id)
        {
            using (var clienteHTTP = new HttpClient())
            {
                clienteHTTP.BaseAddress = new Uri(_mbmaConfig["Services:URL"]);
                var mensaje = clienteHTTP.DeleteAsync($"incidentes/{id}").Result;

                return mensaje.IsSuccessStatusCode;
            }
        }

        #endregion

        public IActionResult Index(int page = 1, int idEstadoIncidente = 0, int numreg = 15)
        {
            var listado = obtenerIncidente();

            if (idEstadoIncidente > 0)
                listado = listado.Where(c => c.EstadoIncidente.Id == idEstadoIncidente).ToList();

            var lstEstadoInc = obtenerEstadosIncidente();
            lstEstadoInc.Insert(0, new EstadoIncidente() { Id = 0, NombreEstado = "--SELECCIONE--" });

            int totalRegistros = listado.Count();
            int registrosPorPaginas = numreg;

            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPaginas);
            int omitir = registrosPorPaginas * (page - 1);

            //VIEWBAG
            ViewBag.totalPaginas = totalPaginas;
            ViewBag.estadoIncidente = new SelectList(lstEstadoInc, "Id", "NombreEstado", idEstadoIncidente);

            ViewBag.registroSeleccionado = numreg;
            ViewBag.EstadoIncidenteSeleccionado = idEstadoIncidente;

            return View(listado.Skip(omitir).Take(registrosPorPaginas));
        }

        public IActionResult Create()
        {
            var lstEstadoInc = obtenerEstadosIncidente();
            lstEstadoInc.Insert(0, new EstadoIncidente() { Id = 0, NombreEstado = "--SELECCIONE--" });
            ViewBag.estadoIncidente = new SelectList(lstEstadoInc, "Id", "NombreEstado");

            return View(new Incidente());
        }

        [HttpPost]
        public IActionResult Create(Incidente incidente)
        {
            Incidente nuevoID = registrarIncidente(incidente);
            return RedirectToAction("Details", new { id = nuevoID.Id });
        }

        public IActionResult Edit(int id)
        {
            var incidente = obtenerPorId(id);
            var lstEstadoInc = obtenerEstadosIncidente();
            lstEstadoInc.Insert(0, new EstadoIncidente() { Id = 0, NombreEstado = "--SELECCIONE--" });

            ViewBag.estadoIncidente = new SelectList(lstEstadoInc, "Id", "NombreEstado");
            return View(incidente);
        }

        [HttpPost]
        public IActionResult Edit(Incidente incidente)
        {
            actualizarIncidente(incidente);
            return RedirectToAction("Details", new { id = incidente.Id });
        }

        public IActionResult Details(int id)
        {
            var mbmaProducto = obtenerPorId(id);
            return View(mbmaProducto);
        }

        public IActionResult Delete(int id)
        {
            var mbmaProducto = obtenerPorId(id);
            return View(mbmaProducto);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            eliminarProducto(id);
            return RedirectToAction("index");
        }

    }
}

