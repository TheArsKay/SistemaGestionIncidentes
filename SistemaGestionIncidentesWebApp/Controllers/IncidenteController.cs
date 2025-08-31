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

        #region Helpers (consumo API)

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

        private List<Usuario> obtenerUsuario()
        {
            var lstUsuario = new List<Usuario>();
            using (var clienteHTTP = new HttpClient())
            {
                clienteHTTP.BaseAddress = new Uri(_mbmaConfig["Services:URL"]);

                var mensaje = clienteHTTP.GetAsync("Usuarios").Result;

                var data = mensaje.Content.ReadAsStringAsync().Result;

                lstUsuario = JsonConvert.DeserializeObject<List<Usuario>>(data);
            }
            return lstUsuario;
        }

        private List<Categoria> obtenerCategoria()
        {
            var lstCategoria = new List<Categoria>();
            using (var clienteHTTP = new HttpClient())
            {
                clienteHTTP.BaseAddress = new Uri(_mbmaConfig["Services:URL"]);

                var mensaje = clienteHTTP.GetAsync("Categoria/listar").Result;

                var data = mensaje.Content.ReadAsStringAsync().Result;

                lstCategoria = JsonConvert.DeserializeObject<List<Categoria>>(data);
            }
            return lstCategoria;
        }

        private List<Tecnico> obtenerTecnico()
        {
            var lstTecnico = new List<Tecnico>();
            using (var clienteHTTP = new HttpClient())
            {
                clienteHTTP.BaseAddress = new Uri(_mbmaConfig["Services:URL"]);

                var mensaje = clienteHTTP.GetAsync("Tecnico/listar").Result;

                var data = mensaje.Content.ReadAsStringAsync().Result;

                lstTecnico = JsonConvert.DeserializeObject<List<Tecnico>>(data);
            }
            return lstTecnico;
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

        private bool eliminarIncidente(int id)
        {
            using (var clienteHTTP = new HttpClient())
            {
                clienteHTTP.BaseAddress = new Uri(_mbmaConfig["Services:URL"]);
                var mensaje = clienteHTTP.DeleteAsync($"incidentes/{id}").Result;

                return mensaje.IsSuccessStatusCode;
            }
        }

        #endregion

        #region Metodos crud que generarán la vista


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

            var lstUsuario = obtenerUsuario();
            lstUsuario.Insert(0, new Usuario() { Id = 0, Nombre = "--SELECCIONE--" });
            ViewBag.usuario = new SelectList(lstUsuario, "Id", "Nombre");

            var lstCategoria = obtenerCategoria();
            lstCategoria.Insert(0, new Categoria() { Id = 0, Nombre = "--SELECCIONE--" });
            ViewBag.categoria = new SelectList(lstCategoria, "Id", "Nombre");

            var lstTecnico = obtenerTecnico();
            lstTecnico.Insert(0, new Tecnico() { Id = 0, Nombre  = "--SELECCIONE--" });
            ViewBag.tecnico = new SelectList(lstTecnico, "Id", "Nombre");


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

            var lstUsuario = obtenerUsuario();
            lstUsuario.Insert(0, new Usuario() { Id = 0, Nombre = "--SELECCIONE--" });
            ViewBag.usuario = new SelectList(lstUsuario, "Id", "Nombre");

            var lstCategoria = obtenerCategoria();
            lstCategoria.Insert(0, new Categoria() { Id = 0, Nombre = "--SELECCIONE--" });
            ViewBag.categoria = new SelectList(lstCategoria, "Id", "Nombre");

            var lstTecnico = obtenerTecnico();
            lstTecnico.Insert(0, new Tecnico() { Id = 0, Nombre = "--SELECCIONE--" });
            ViewBag.tecnico = new SelectList(lstTecnico, "Id", "Nombre");

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

            var lstEstadoInc = obtenerEstadosIncidente();
            lstEstadoInc.Insert(0, new EstadoIncidente() { Id = 0, NombreEstado = "--SELECCIONE--" });
            ViewBag.estadoIncidente = new SelectList(lstEstadoInc, "Id", "NombreEstado");

            var lstUsuario = obtenerUsuario();
            lstUsuario.Insert(0, new Usuario() { Id = 0, Nombre = "--SELECCIONE--" });
            ViewBag.usuario = new SelectList(lstUsuario, "Id", "Nombre");

            var lstCategoria = obtenerCategoria();
            lstCategoria.Insert(0, new Categoria() { Id = 0, Nombre = "--SELECCIONE--" });
            ViewBag.categoria = new SelectList(lstCategoria, "Id", "Nombre");

            var lstTecnico = obtenerTecnico();
            lstTecnico.Insert(0, new Tecnico() { Id = 0, Nombre = "--SELECCIONE--" });
            ViewBag.tecnico = new SelectList(lstTecnico, "Id", "Nombre");

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
            eliminarIncidente(id);
            return RedirectToAction("index");
        }
        #endregion
    }
}