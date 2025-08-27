using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SistemaGestionIncidentesWebApp.Models;

namespace SistemaGestionIncidentesWebApp.Controllers
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

                lstIncidente = JsonConvert.DeserializeObject<List<Incidente>>(data) ?? new List<Incidente>();
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

                lstEstadoIncidente = JsonConvert.DeserializeObject<List<EstadoIncidente>>(data) ?? new List<EstadoIncidente>();
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
                incidente = JsonConvert.DeserializeObject<Incidente>(data) ?? new Incidente();
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
                nuevo = JsonConvert.DeserializeObject<Incidente>(data) ?? new Incidente();
            }

            return nuevo;
        }

        private Incidente actualizarIncidente(Incidente incidente)
        {
            if (incidente == null) return null;

            // Extraer idEstadoIncidente del modelo o del formulario
            int idEstado = 0;
            try
            {
                idEstado = incidente?.EstadoIncidente?.Id ?? 0;
                if (idEstado == 0 && Request?.Form != null && Request.Form.ContainsKey("idEstadoIncidente"))
                {
                    int.TryParse(Request.Form["idEstadoIncidente"].FirstOrDefault(), out idEstado);
                }
            }
            catch
            {
                idEstado = 0;
            }

            // Recuperar el incidente actual desde la API (fallback values)
            Incidente existente = null;
            try
            {
                existente = obtenerPorId(incidente.Id) ?? new Incidente();
            }
            catch
            {
                existente = new Incidente();
            }

            // Si la vista no envió Descripcion o Solucion, usar las del existente (evita validation 400)
            var descripcion = incidente.DescripcionIncidente ?? existente.DescripcionIncidente ?? string.Empty;
            var solucion = incidente.SolucionIncidente ?? existente.SolucionIncidente ?? string.Empty;

            // Determinar ids (si no llegan o son 0, los trataremos condicionadamente)
            int idCategoria = incidente?.Categoria?.Id ?? existente?.Categoria?.Id ?? 0;
            int idUsuarioTecnico = incidente?.UsuarioTecnico?.Id ?? existente?.UsuarioTecnico?.Id ?? 0;
            int idUsuarioReporta = incidente?.UsuarioReporta?.Id ?? existente?.UsuarioReporta?.Id ?? 0;

            // Construir payload dinámico (omitimos campos FK que serían 0 para no romper FK)
            var job = new JObject();

            // Título: enviar si existe (fallback desde existente)
            var titulo = incidente?.TituloIncidente ?? existente?.TituloIncidente;
            if (!string.IsNullOrWhiteSpace(titulo))
                job["TituloIncidente"] = titulo;

            // Descripción y solución: siempre enviar (API exige no nulos)
            job["DescripcionIncidente"] = descripcion;
            job["SolucionIncidente"] = solucion;

            // Estado: enviar si está presente (>0)
            if (idEstado > 0)
                job["idEstadoIncidente"] = idEstado;

            // Solo añadir FK si tienen valor válido (>0)
            if (idCategoria > 0)
                job["idCategoria"] = idCategoria;

            if (idUsuarioTecnico > 0)
                job["idUsuarioTecnico"] = idUsuarioTecnico;

            if (idUsuarioReporta > 0)
                job["idUsuarioReporta"] = idUsuarioReporta;

            using (var clienteHTTP = new HttpClient())
            {
                clienteHTTP.BaseAddress = new Uri(_mbmaConfig["Services:URL"]);

                var contenido = new StringContent(JsonConvert.SerializeObject(job),
                    System.Text.Encoding.UTF8, "application/json");

                var mensaje = clienteHTTP.PutAsync($"incidentes/{incidente.Id}", contenido).Result;
                var data = mensaje.Content.ReadAsStringAsync().Result;

                if (mensaje.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(data))
                {
                    try
                    {
                        var actualizado = JsonConvert.DeserializeObject<Incidente>(data);
                        return actualizado ?? incidente;
                    }
                    catch
                    {
                        return incidente;
                    }
                }
                else
                {
                    // log para depuración
                    System.Diagnostics.Debug.WriteLine($"PUT /incidentes/{incidente.Id} -> {(int)mensaje.StatusCode} {mensaje.ReasonPhrase}");
                    System.Diagnostics.Debug.WriteLine("Request payload: " + job.ToString(Newtonsoft.Json.Formatting.None));
                    System.Diagnostics.Debug.WriteLine("Response: " + data);
                    return incidente;
                }
            }
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

        // ----------------------
        // Nuevos métodos privados (pegarlos aquí dentro del mismo region)
        // ----------------------

        /// <summary>
        /// Llama al API para obtener la lista de Incidente (modelo completo) filtrada por técnico.
        /// </summary>
        private List<Incidente> obtenerIncidentesPorTecnico(int tecnicoId)
        {
            var lista = new List<Incidente>();
            try
            {
                using (var clienteHTTP = new HttpClient())
                {
                    clienteHTTP.BaseAddress = new Uri(_mbmaConfig["Services:URL"]);

                    // intentamos ambas formas de ruta por compatibilidad
                    var rutas = new[]
                    {
                $"incidentes/tecnico/{tecnicoId}",
                $"api/incidentes/tecnico/{tecnicoId}"
            };

                    HttpResponseMessage mensaje = null;
                    string data = "";

                    foreach (var ruta in rutas)
                    {
                        mensaje = clienteHTTP.GetAsync(ruta).Result;
                        data = mensaje.Content.ReadAsStringAsync().Result ?? "";
                        // guardamos el primer intento (útil para debug)
                        TempData["MisIncidentes_Status"] = $"{ruta} -> {(int)mensaje.StatusCode} {mensaje.ReasonPhrase}";
                        TempData["MisIncidentes_RawJsonPreview"] = data.Length > 1000 ? data.Substring(0, 1000) + "..." : data;

                        // si 200 OK o 204 No Content, lo procesamos
                        if (mensaje.IsSuccessStatusCode || mensaje.StatusCode == System.Net.HttpStatusCode.NoContent)
                            break;
                    }

                    // si la respuesta fue exitosa y el body parece JSON, intentar deserializar a List<Incidente>
                    if (mensaje != null && mensaje.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(TempData["MisIncidentes_RawJsonPreview"] as string))
                    {
                        try
                        {
                            // primero intentar deserializar directo a List<Incidente>
                            lista = JsonConvert.DeserializeObject<List<Incidente>>(data) ?? new List<Incidente>();

                            // si vino un objeto contenedor en lugar de array, intentar recuperar propiedad que sea array
                            if (lista.Count == 0)
                            {
                                var root = JToken.Parse(string.IsNullOrWhiteSpace(data) ? "{}" : data);
                                if (root.Type == JTokenType.Object)
                                {
                                    var obj = (JObject)root;
                                    var candidate = obj.Properties()
                                                       .Select(p => p.Value)
                                                       .FirstOrDefault(v => v != null && v.Type == JTokenType.Array);
                                    if (candidate != null)
                                    {
                                        lista = candidate.ToObject<List<Incidente>>() ?? new List<Incidente>();
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // no parseó a List<Incidente> — lo manejaremos con fallback
                            lista = new List<Incidente>();
                        }
                    }

                    // Fallback: si no obtuvimos nada útil del endpoint, traer todos y filtrar por UsuarioTecnico.Id
                    if (lista == null || lista.Count == 0)
                    {
                        try
                        {
                            var todos = obtenerIncidente(); // ya existente
                            lista = todos.Where(i => i.UsuarioTecnico != null && i.UsuarioTecnico.Id == tecnicoId).ToList();

                            if (lista.Count == 0)
                            {
                                TempData["MisIncidentes_Info"] = "Fallback: no se encontraron incidentes tras traer todos y filtrar por UsuarioTecnico.Id.";
                            }
                            else
                            {
                                TempData["MisIncidentes_Info"] = $"Fallback: {lista.Count} incidentes encontrados filtrando en cliente.";
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["MisIncidentes_Error"] = "Fallback error: " + ex.Message;
                            lista = new List<Incidente>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["MisIncidentes_Error"] = ex.Message;
                System.Diagnostics.Debug.WriteLine("obtenerIncidentesPorTecnico error: " + ex.ToString());
                lista = new List<Incidente>();
            }
            return lista;
        }

        /// <summary>
        /// Mapea un Incidente (completo) a IncidenteListado (la vista de lista).
        /// Si tu modelo Incidente tiene Codigo_Ticket, cámbialo aquí por inc.Codigo_Ticket.
        /// </summary>
        private IncidenteListado MapToListado(Incidente inc)
        {
            if (inc == null) return new IncidenteListado();

            return new IncidenteListado
            {
                // Si tienes la propiedad Codigo_Ticket en Incidente, utiliza inc.Codigo_Ticket; si no, usa Id.
                Codigo_Ticket = /*inc.Codigo_Ticket*/ inc.Id,
                Titulo_Incidente = inc.TituloIncidente ?? string.Empty,
                Usuario_Reporta = inc.UsuarioReporta?.Nombre ?? string.Empty,
                Estado = inc.EstadoIncidente?.NombreEstado ?? string.Empty,
                Tecnico_Asignado = inc.UsuarioTecnico?.Nombre ?? string.Empty
            };
        }

        #endregion

        public IActionResult Index(int page = 1, int idEstadoIncidente = 0, int numreg = 15)
        {
            var listado = obtenerIncidente();

            if (idEstadoIncidente > 0)
                listado = listado.Where(c => c.EstadoIncidente != null && c.EstadoIncidente.Id == idEstadoIncidente).ToList();

            var lstEstadoInc = obtenerEstadosIncidente();
            lstEstadoInc.Insert(0, new EstadoIncidente() { Id = 0, NombreEstado = "--SELECCIONE--" });

            int totalRegistros = listado.Count();
            int registrosPorPaginas = numreg;

            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPaginas);
            int omitir = registrosPorPaginas * (page - 1);

            // VIEWBAG
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
            return RedirectToAction("Index");
        }

        // Reemplazado: MisIncidentes ahora usa el mapeo Incidente -> IncidenteListado
        public IActionResult MisIncidentes()
        {
            var tecnicoId = HttpContext.Session.GetInt32("UsuarioId");
            if (tecnicoId == null)
                return RedirectToAction("Login", "Usuarios"); // no está logueado

            // obtener la lista de Incidente (modelo completo) del API filtrada por técnico (o fallback)
            var incidentes = obtenerIncidentesPorTecnico(tecnicoId.Value);

            // diagnóstico opcional si viene vacío
            if (incidentes == null || incidentes.Count == 0)
            {
                // TempData["MisIncidentes_*"] ya contiene info para debug desde obtenerIncidentesPorTecnico
                return View("MisIncidentes", new List<IncidenteListado>());
            }

            // mapear a IncidenteListado (lo que espera la vista)
            var listado = incidentes.Select(i => MapToListado(i)).ToList();

            return View("MisIncidentes", listado);
        }

        [HttpGet]
        public IActionResult DebugSession()
        {
            var id = HttpContext.Session.GetInt32("UsuarioId");
            var nombre = HttpContext.Session.GetString("UsuarioNombre");
            var rol = HttpContext.Session.GetString("UsuarioRol");
            return Content($"UsuarioId={id ?? -1}, UsuarioNombre={nombre ?? "null"}, UsuarioRol={rol ?? "null"}");
        }
    }
}
