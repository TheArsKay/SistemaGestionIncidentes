using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using SistemaGestionIncidentesWebApp.Models;

namespace SistemaGestionIncidentesWebApp.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IConfiguration _config;
        public UsuariosController(IConfiguration config)
        {
            _config = config;
        }
        #region _METODOS PRIVADOS_
        private List<Usuario> obtenerUsuarios()
        {
            var listado = new List<Usuario>();
            // TODO: Aplicar lógica para traer los datos desde el servicio
            // Declarar un cliente HTTP
            using (var usuarioHTTP = new HttpClient())
            {
                // Definir Dirección/URL Base
                usuarioHTTP.BaseAddress = new Uri(_config["Services:URL"]);

                // Obtener el mensaje de respuesta
                var mensaje = usuarioHTTP.GetAsync("Usuarios").Result;

                // Leer el contenido
                var data = mensaje.Content.ReadAsStringAsync().Result;

                // Convertir los datos de tipo string(Json) en objeto
                listado = JsonConvert.DeserializeObject<List<Usuario>>(data);
            }
            return listado;

        }
        private Usuario registrarUsuario(Usuario usuario)
        {
            Usuario nuevoUsuario = null;
            using (var usuarioHTTP = new HttpClient())
            {
                usuarioHTTP.BaseAddress = new Uri(_config["Services:URL"]);

                StringContent contenido = new StringContent(JsonConvert.SerializeObject(usuario),
                    System.Text.Encoding.UTF8, "application/json");

                //   var mensaje = usuarioHTTP.PostAsync("Usuarios", contenido).Result;
                var respuesta = usuarioHTTP.PostAsync("Usuarios/registrar", contenido).Result;

                if (!respuesta.IsSuccessStatusCode)
                {
                    var error = respuesta.Content.ReadAsStringAsync().Result;
                    throw new Exception($"Error en API: {respuesta.StatusCode}");
                }

                var data = respuesta.Content.ReadAsStringAsync().Result;
                //var data = mensaje.Content.ReadAsStringAsync().Result;
                nuevoUsuario = JsonConvert.DeserializeObject<Usuario>(data);

            }

            return nuevoUsuario;
        }
        private Usuario iniciarSesion(string email, string clave)
        {
            Usuario usuario = null;
            using (var usuarioHTTP = new HttpClient())
            {
                usuarioHTTP.BaseAddress = new Uri(_config["Services:URL"]);

                // Aquí la diferencia: enviar como query params, no como JSON
                var respuesta = usuarioHTTP.PostAsync(
                    $"Usuarios/iniciar_sesion?email={email}&clave={clave}", null
                ).Result;

                if (!respuesta.IsSuccessStatusCode)
                {
                    var error = respuesta.Content.ReadAsStringAsync().Result;
                    throw new Exception($"Error en API: {respuesta.StatusCode}");
                }

                var data = respuesta.Content.ReadAsStringAsync().Result;
                usuario = JsonConvert.DeserializeObject<Usuario>(data);
            }
            return usuario;
        }
        private List<Rol> obtenerRoles()
        {
            var roles = new List<Rol>();
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(_config["Services:URL"]);
                var msg = http.GetAsync("Usuarios/roles").Result; 
                var data = msg.Content.ReadAsStringAsync().Result;
                roles = JsonConvert.DeserializeObject<List<Rol>>(data);
            }
            return roles;
        }

        #endregion

        public IActionResult Register()
        {
            var roles = obtenerRoles()
                .Where(r => r.Id != 1)
                .ToList();

            ViewBag.Roles = new SelectList(roles, "Id", "Nombre");
            return View(new Usuario());
        }

        [HttpPost]
        public IActionResult Register(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(obtenerRoles(), "Id", "Nombre");
                return View(usuario);
            }

            var nuevo = registrarUsuario(usuario);
            if (nuevo == null)
            {
                ViewBag.Roles = new SelectList(obtenerRoles(), "Id", "Nombre");
                ModelState.AddModelError("", "No se pudo registrar el usuario.");
                return View(usuario);
            }
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string clave)
        {
            var usuario = iniciarSesion(email, clave);

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                return View();
            }

            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
            HttpContext.Session.SetString("UsuarioRol", usuario.nombreRol);
            HttpContext.Session.SetInt32("UsuarioId", usuario.Id ?? 0);


            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            // Limpia la sesión
            HttpContext.Session.Clear();

            // Redirige al login
            return RedirectToAction("Login", "Usuarios");
        }
    }
}

