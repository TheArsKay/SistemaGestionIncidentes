using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SistemaGestionIncidentesWebApp.Models;

namespace SistemaGestionIncidentesWebApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IConfiguration _config;
        public DashboardController(IConfiguration config)
        {
            _config = config;
        }

        #region _ METODOS PRIVADOS _

        // Método privado para obtener incidentes desde el API
        private List<IncidenteListado> obtenerIncidentes()
        {
            var lista = new List<IncidenteListado>();

            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var msg = http.GetAsync("incidentes/listarResumen").Result;

                if (!msg.IsSuccessStatusCode)
                {
                    throw new Exception("Error al consumir el API de incidentes.");
                }

                var data = msg.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<IncidenteListado>>(data);
            }

            return lista;
        }



        #endregion


        public IActionResult Index()
        {
            // Obtener datos de sesión
            var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Usuario";
            var rol = HttpContext.Session.GetString("UsuarioRol") ?? "Rol";

            // Configurar opciones de menú según rol
            var opciones = new List<(string Texto, string Controller, string Action)>();

            switch (rol)
            {
                case "Operador":
                    opciones.Add(("📋 Listado de Incidentes", "Incidentes", "Index"));
                    opciones.Add(("➕ Registrar Incidente", "Incidentes", "Create"));
                    break;

                case "Supervisor":
                    opciones.Add(("📊 Gestión de Incidentes", "Incidentes", "Index"));
                    opciones.Add(("📈 Reportes", "Reportes", "Index")); // vista futura
                    break;

                case "Técnico":
                    opciones.Add(("🛠 Incidentes Asignados", "Incidentes", "Asignados")); // vista futura
                    opciones.Add(("✏️ Actualizar Estado", "Incidentes", "Actualizar"));  // vista futura
                    break;
            }

            // Conteo de incidentes
            var incidentes = obtenerIncidentes();
            var pendientes = incidentes.Count(i => i.Estado == "Pendiente");
            var enProceso = incidentes.Count(i => i.Estado == "En Proceso");
            var cerrados = incidentes.Count(i => i.Estado == "Cerrado");

            // Pasar datos al layout
            ViewBag.Usuario = usuario;
            ViewBag.Rol = rol;
            ViewBag.Opciones = opciones;

            ViewBag.Pendientes = pendientes;
            ViewBag.EnProceso = enProceso;
            ViewBag.Cerrados = cerrados;

            return View();
        }

    }
}
