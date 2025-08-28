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
        private readonly IConfiguration _config;

        public IncidenteController(IConfiguration config)
        {
            _config = config;
        }

        private string BaseUrl
        {
            get
            {
                var url = _config["Services:URL"]?.TrimEnd('/') ?? "";
                while (url.EndsWith("/api/api", StringComparison.OrdinalIgnoreCase))
                {
                    url = url.Substring(0, url.Length - "/api".Length);
                }
                if (!url.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
                    url = url + "/api";
                return url;
            }
        }

        #region Métodos de consumo al API (helpers)

        private List<IncidenteListado> obtenerIncidentesResumen()
        {
            var lista = new List<IncidenteListado>();
            using (var http = new HttpClient())
            {
                var url = $"{BaseUrl}/incidentes/listarResumen";
                var msg = http.GetAsync(url).Result;
                if (!msg.IsSuccessStatusCode) return lista;
                var data = msg.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<IncidenteListado>>(data) ?? new List<IncidenteListado>();
            }
            return lista;
        }

        private List<Incidente> obtenerIncidentes()
        {
            var lista = new List<Incidente>();
            using (var http = new HttpClient())
            {
                var url = $"{BaseUrl}/incidentes";
                var msg = http.GetAsync(url).Result;
                if (!msg.IsSuccessStatusCode) return lista;

                var data = msg.Content.ReadAsStringAsync().Result ?? "[]";

                try
                {
                    lista = JsonConvert.DeserializeObject<List<Incidente>>(data) ?? new List<Incidente>();
            }
                catch
                {
                    try
                    {
                        var arr = JArray.Parse(data);
                        foreach (var token in arr)
                        {
                            try
                            {
                                var inc = token.ToObject<Incidente>() ?? new Incidente();

                                var ut = token["usuarioTecnico"] ?? token["usuario_tecnico"] ?? token["tecnico"];
                                if (ut != null && ut.Type == JTokenType.Object)
                                    inc.UsuarioTecnico = ut.ToObject<Usuario>();

                                var ur = token["usuarioReporta"] ?? token["usuario_reporta"];
                                if (ur != null && ur.Type == JTokenType.Object)
                                    inc.UsuarioReporta = ur.ToObject<Usuario>();

                                var cat = token["categoria"] ?? token["categoria_obj"];
                                if (cat != null && cat.Type == JTokenType.Object)
                                    inc.Categoria = cat.ToObject<Categoria>();

                                var est = token["estadoIncidente"] ?? token["estado_incidente"] ?? token["estado"];
                                if (est != null && est.Type == JTokenType.Object)
                                    inc.EstadoIncidente = est.ToObject<EstadoIncidente>();

                                var fc = token.SelectToken("fechaCreacion") ?? token.SelectToken("fecha_creacion");
                                if (fc != null && DateTime.TryParse(fc.ToString(), out var fcd)) inc.FechaCreacion = fcd;

                                var fm = token.SelectToken("fechaModificacion") ?? token.SelectToken("fecha_modificacion");
                                if (fm != null && DateTime.TryParse(fm.ToString(), out var fmd)) inc.FechaModificacion = fmd;

                                lista.Add(inc);
        }
                            catch
        {
                            }
                        }
                    }
                    catch
            {
                        lista = new List<Incidente>();
                    }
                }
            }
            return lista;
        }

        [HttpGet]
        private Incidente obtenerPorId(int id)
        {
            try
            {
                using (var http = new HttpClient())
                {
                    var url = $"{BaseUrl}/incidentes/{id}";
                    var msg = http.GetAsync(url).Result;
                    if (!msg.IsSuccessStatusCode) return null;

                    var raw = msg.Content.ReadAsStringAsync().Result ?? "";
                    if (string.IsNullOrWhiteSpace(raw)) return null;

                    JToken root = null;
                    try
                    {
                        var parsed = JToken.Parse(raw);
                        if (parsed.Type == JTokenType.Array)
                            root = parsed.First ?? new JObject();
                        else
                            root = parsed;
                    }
                    catch
                    {
                        return null; 
            }

                    var incidente = new Incidente();
                    incidente.Id = root.Value<int?>("id") ?? root.Value<int?>("Id") ?? 0;
                    incidente.TituloIncidente = root.Value<string>("tituloIncidente")
                                               ?? root.Value<string>("titulo")
                                               ?? root.Value<string>("titulo_incidente")
                                               ?? "";
                    incidente.DescripcionIncidente = root.Value<string>("descripcionIncidente")
                                                    ?? root.Value<string>("descripcion_incidente")
                                                    ?? "";
                    incidente.SolucionIncidente = root.Value<string>("solucionIncidente")
                                                 ?? root.Value<string>("solucion_incidente")
                                                 ?? null;


                    var fc = root.Value<DateTime?>("fechaCreacion") ?? root.Value<DateTime?>("fecha_creacion");
                    if (fc != null) incidente.FechaCreacion = fc.Value;
                    var fm = root.Value<DateTime?>("fechaModificacion") ?? root.Value<DateTime?>("fecha_modificacion");
                    incidente.FechaModificacion = fm;

                    int idUsuario = root.Value<int?>("idUsuario")
                                  ?? root.Value<int?>("idUsuarioReporta")
                                  ?? root.Value<int?>("id_usuario")
                                  ?? 0;
                    int idCategoria = root.Value<int?>("idCategoria")
                                   ?? root.Value<int?>("id_categoria")
                                   ?? 0;
                    int idEstado = root.Value<int?>("idEstadoIncidente")
                                 ?? root.Value<int?>("id_estado")
                                 ?? root.Value<int?>("idEstado")
                                 ?? 0;
                    int idTecnico = root.Value<int?>("idTecnico")
                                  ?? root.Value<int?>("idUsuarioTecnico")
                                  ?? root.Value<int?>("id_tecnico")
                                  ?? 0;

                    var usuarios = obtenerUsuarios() ?? new List<Usuario>();
                    var tecnicos = obtenerTecnicos() ?? new List<Tecnico>();
                    var categorias = obtenerCategorias() ?? new List<Categoria>();
                    var estados = obtenerEstados() ?? new List<EstadoIncidente>();

                    var usr = usuarios.FirstOrDefault(u => u.Id == idUsuario);
                    if (usr != null)
                    {
                        incidente.UsuarioReporta = usr;
        }
                    else
                    {

                        var nombreUsr = root.SelectToken("usuarioReporta.nombre")?.ToString()
                                     ?? root.SelectToken("usuario_reporta.nombre")?.ToString()
                                     ?? root.SelectToken("usuarioReporta")?.ToString();
                        if (!string.IsNullOrWhiteSpace(nombreUsr))
                            incidente.UsuarioReporta = new Usuario { Id = idUsuario, Nombre = nombreUsr };
                    }
    
                    var techAsUsuario = usuarios.FirstOrDefault(u => u.Id == idTecnico);
                    if (techAsUsuario != null)
                    {
                        incidente.UsuarioTecnico = techAsUsuario;
                    }
                    else
                    {
                        var techFromList = tecnicos.FirstOrDefault(t => t.Id == idTecnico);
                        if (techFromList != null)
                        {
                            incidente.UsuarioTecnico = new Usuario { Id = techFromList.Id, Nombre = techFromList.Nombre };
                        }
                        else
                        {
                            var nombreTech = root.SelectToken("usuarioTecnico.nombre")?.ToString()
                                          ?? root.SelectToken("usuario_tecnico.nombre")?.ToString()
                                          ?? root.SelectToken("usuarioTecnico")?.ToString();
                            if (!string.IsNullOrWhiteSpace(nombreTech))
                                incidente.UsuarioTecnico = new Usuario { Id = idTecnico, Nombre = nombreTech };
                        }
                    }

                    
                    var cat = categorias.FirstOrDefault(c => c.Id == idCategoria);
                    if (cat != null) incidente.Categoria = cat;
                    else
        {
                        var nombreCat = root.SelectToken("categoria.nombre")?.ToString()
                                     ?? root.SelectToken("categoria")?.ToString();
                        if (!string.IsNullOrWhiteSpace(nombreCat))
                            incidente.Categoria = new Categoria { Id = idCategoria, Nombre = nombreCat };
                    }

          
                    var est = estados.FirstOrDefault(e => e.Id == idEstado);
                    if (est != null) incidente.EstadoIncidente = est;
                    else
            {
                        var nombreEst = root.SelectToken("estadoIncidente.nombreEstado")?.ToString()
                                      ?? root.SelectToken("estadoIncidente.nombre")?.ToString()
                                      ?? root.SelectToken("nombreEstado")?.ToString()
                                      ?? root.SelectToken("estado")?.ToString();
                        if (!string.IsNullOrWhiteSpace(nombreEst))
                            incidente.EstadoIncidente = new EstadoIncidente { Id = idEstado, NombreEstado = nombreEst };
            }

            return incidente;
        }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("obtenerPorId error: " + ex.ToString());
                return null;
            }
        }




        [HttpPost]
        private Incidente registrarIncidente(Incidente incidente)
        {
            using (var http = new HttpClient())
            {
                var url = $"{BaseUrl}/incidentes";
                var msg = http.PostAsJsonAsync(url, incidente).Result;
                if (!msg.IsSuccessStatusCode) throw new Exception("Error al registrar el incidente.");
                var data = msg.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<Incidente>(data);
            }
        }

        [HttpPut]
        private Incidente actualizarIncidente(Incidente incidente)
        {
            if (incidente == null) return null;

            int GetId(object src, params string[] names)
            {
                foreach (var n in names)
                {
                    var p = src.GetType().GetProperty(n);
                    if (p == null) continue;
                    var v = p.GetValue(src);
                    if (v == null) continue;
                    if (v is int iv) return iv;
                    if (int.TryParse(v.ToString(), out var parsed)) return parsed;
            }
                return 0;
            }

            var payload = new
            {
                Id = incidente.Id,
                tituloIncidente = incidente.TituloIncidente,
                descripcionIncidente = incidente.DescripcionIncidente,
                solucionIncidente = incidente.SolucionIncidente,
                idUsuarioReporta = incidente.UsuarioReporta?.Id ?? GetId(incidente, "idUsuarioReporta", "idUsuario", "id_usuario"),
                idCategoria = incidente.Categoria?.Id ?? GetId(incidente, "idCategoria", "id_categoria"),
                idEstadoIncidente = incidente.EstadoIncidente?.Id ?? GetId(incidente, "idEstadoIncidente", "id_estado"),
                idUsuarioTecnico = incidente.UsuarioTecnico?.Id ?? GetId(incidente, "idUsuarioTecnico", "idTecnico", "id_tecnico")
            };

            using (var http = new HttpClient())
            {
                var url = $"{BaseUrl}/incidentes/{incidente.Id}";
                var msg = http.PutAsJsonAsync(url, payload).Result;

                if (!msg.IsSuccessStatusCode)
                {
                    var resp = msg.Content.ReadAsStringAsync().Result;
                    throw new Exception($"Error al actualizar incidente. Status {(int)msg.StatusCode}. Response: {resp}");
                }

                var data = msg.Content.ReadAsStringAsync().Result;
                try { return JsonConvert.DeserializeObject<Incidente>(data); }
                catch { return incidente; }
            }
        }


        private bool eliminarIncidente(int id)
        {
            using (var http = new HttpClient())
            {
                var url = $"{BaseUrl}/incidentes/{id}";
                var msg = http.DeleteAsync(url).Result;
                if (!msg.IsSuccessStatusCode) throw new Exception("Error al eliminar el incidente.");
                var data = msg.Content.ReadAsStringAsync().Result;
                try { return JsonConvert.DeserializeObject<bool>(data); }
                catch { return msg.IsSuccessStatusCode; }
            }
        }

        private List<Usuario> obtenerUsuarios()
        {
            var lista = new List<Usuario>();
            using (var http = new HttpClient())
            {
                var url = $"{BaseUrl}/usuarios";
                var msg = http.GetAsync(url).Result;
                if (!msg.IsSuccessStatusCode) return lista;
                var data = msg.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<Usuario>>(data) ?? new List<Usuario>();
            }
            return lista;
        }
                
        private List<Tecnico> obtenerTecnicos()
        {
            var lista = new List<Tecnico>();
            using (var http = new HttpClient())
            {
                var url = $"{BaseUrl}/tecnico/listar";
                var msg = http.GetAsync(url).Result;
                if (!msg.IsSuccessStatusCode) return lista;
                var data = msg.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<Tecnico>>(data) ?? new List<Tecnico>();
            }
            return lista;
        }

        private List<EstadoIncidente> obtenerEstados()
        {
            var lista = new List<EstadoIncidente>();
            using (var http = new HttpClient())
            {
                var url = $"{BaseUrl}/estados-incidente";
                var msg = http.GetAsync(url).Result;
                if (!msg.IsSuccessStatusCode) return lista;
                var data = msg.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<EstadoIncidente>>(data) ?? new List<EstadoIncidente>();
            }
            return lista;
        }

        private List<EstadoIncidente> obtenerEstadosIncidente() => obtenerEstados();

        private List<Categoria> obtenerCategorias()
        {
            var lista = new List<Categoria>();
            using (var http = new HttpClient())
            {
                var url = $"{BaseUrl}/categoria/listar";
                var msg = http.GetAsync(url).Result;
                if (!msg.IsSuccessStatusCode) return lista;
                var data = msg.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<Categoria>>(data) ?? new List<Categoria>();
            }
            return lista;
        }

  
        public IActionResult Details(int id)
        {
            var incidente = obtenerPorId(id);
            if (incidente == null) return NotFound();
            return View(incidente); 
        }

        [HttpGet]

        public IActionResult Edit(int id)
        {
            var incidente = obtenerPorId(id);
            if (incidente == null) return NotFound();

            if (idEstadoIncidente > 0)
                listado = listado.Where(c => c.EstadoIncidente.Id == idEstadoIncidente).ToList();

            var lstEstadoInc = obtenerEstadosIncidente();
            lstEstadoInc.Insert(0, new EstadoIncidente() { Id = 0, NombreEstado = "--SELECCIONE--" });
            ViewBag.Estados = new SelectList(lstEstadoInc, "Id", "NombreEstado", incidente.EstadoIncidente?.Id ?? incidente.GetType().GetProperty("idEstadoIncidente")?.GetValue(incidente));

            var lstCategorias = obtenerCategorias();
            lstCategorias.Insert(0, new Categoria { Id = 0, Nombre = "--SELECCIONE--" });
            ViewBag.Categorias = new SelectList(lstCategorias, "Id", "Nombre", incidente.Categoria?.Id ?? incidente.GetType().GetProperty("idCategoria")?.GetValue(incidente));

            var lstTecnicos = obtenerTecnicos();
            lstTecnicos.Insert(0, new Tecnico { Id = 0, Nombre = "--SELECCIONE--" });
            ViewBag.Tecnicos = new SelectList(lstTecnicos, "Id", "Nombre", incidente.UsuarioTecnico?.Id ?? incidente.GetType().GetProperty("idUsuarioTecnico")?.GetValue(incidente));

            var lstUsuarios = obtenerUsuarios();
            lstUsuarios.Insert(0, new Usuario { Id = 0, Nombre = "--SELECCIONE--" });
            ViewBag.Usuarios = new SelectList(lstUsuarios, "Id", "Nombre", incidente.UsuarioReporta?.Id ?? incidente.GetType().GetProperty("idUsuarioReporta")?.GetValue(incidente));

            ViewBag.NombreTecnico = incidente.UsuarioTecnico?.Nombre ?? (incidente.GetType().GetProperty("usuarioTecnico")?.GetValue(incidente)?.ToString() ?? "-");

            return View(incidente);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int Id, int idEstadoIncidente, string SolucionIncidente, string DescripcionIncidente, string TituloIncidente, int idUsuarioTecnico = 0, int idUsuarioReporta = 0, int idCategoria = 0)
        {
            var incidente = obtenerPorId(Id);
            if (incidente == null) return NotFound();

            if (string.IsNullOrWhiteSpace(DescripcionIncidente))
                DescripcionIncidente = incidente.DescripcionIncidente ?? string.Empty;

            if (string.IsNullOrWhiteSpace(TituloIncidente))
                TituloIncidente = incidente.TituloIncidente ?? string.Empty;

            void TrySetString(object target, string propName, string value)
            {
                try
                {
                    var prop = target.GetType().GetProperty(propName);
                    if (prop != null && prop.CanWrite) prop.SetValue(target, value);
                }
                catch { }
            }
            void TrySetInt(object target, string propName, int value)
            {
                try
                {
                    var prop = target.GetType().GetProperty(propName);
                    if (prop != null && prop.CanWrite) prop.SetValue(target, value);
                }
                catch { }
            }

            TrySetString(incidente, "TituloIncidente", TituloIncidente);
            TrySetString(incidente, "DescripcionIncidente", DescripcionIncidente);
            TrySetString(incidente, "SolucionIncidente", SolucionIncidente);

            if (idUsuarioTecnico > 0) TrySetInt(incidente, "idUsuarioTecnico", idUsuarioTecnico);
            if (idUsuarioReporta > 0) TrySetInt(incidente, "idUsuarioReporta", idUsuarioReporta);
            if (idCategoria > 0) TrySetInt(incidente, "idCategoria", idCategoria);

            if (idEstadoIncidente > 0)
            {
                TrySetInt(incidente, "idEstadoIncidente", idEstadoIncidente);

                try
                {
                    if (incidente.EstadoIncidente != null)
                    {
                        incidente.EstadoIncidente.Id = idEstadoIncidente;
        }
                    else
                    {
                        var propEstadoObj = incidente.GetType().GetProperty("estadoIncidente") ?? incidente.GetType().GetProperty("EstadoIncidente");
                        if (propEstadoObj != null)
                        {
                            var estadoObj = propEstadoObj.GetValue(incidente);
                            if (estadoObj != null)
                            {
                                var idProp = estadoObj.GetType().GetProperty("Id") ?? estadoObj.GetType().GetProperty("id");
                                if (idProp != null && idProp.CanWrite) idProp.SetValue(estadoObj, idEstadoIncidente);
                                propEstadoObj.SetValue(incidente, estadoObj);
                            }
                        }
                    }
                }
                catch { }
            }

            try
        {
                var actualizado = actualizarIncidente(incidente);
                TempData["EditSuccess"] = "Incidente actualizado correctamente.";
                return RedirectToAction("Details", new { id = Id });
            }
            catch (Exception ex)
            {
            var lstEstadoInc = obtenerEstadosIncidente();
            lstEstadoInc.Insert(0, new EstadoIncidente() { Id = 0, NombreEstado = "--SELECCIONE--" });
                ViewBag.Estados = new SelectList(lstEstadoInc, "Id", "NombreEstado", idEstadoIncidente);

                var lstCategorias = obtenerCategorias();
                lstCategorias.Insert(0, new Categoria { Id = 0, Nombre = "--SELECCIONE--" });
                ViewBag.Categorias = new SelectList(lstCategorias, "Id", "Nombre", idCategoria);

                var lstTecnicos = obtenerTecnicos();
                lstTecnicos.Insert(0, new Tecnico { Id = 0, Nombre = "--SELECCIONE--" });
                ViewBag.Tecnicos = new SelectList(lstTecnicos, "Id", "Nombre", idUsuarioTecnico);

                var lstUsuarios = obtenerUsuarios();
                lstUsuarios.Insert(0, new Usuario { Id = 0, Nombre = "--SELECCIONE--" });
                ViewBag.Usuarios = new SelectList(lstUsuarios, "Id", "Nombre", idUsuarioReporta);

                TempData["EditError"] = "No se pudo actualizar: " + ex.Message;
                ViewBag.NombreTecnico = incidente.UsuarioTecnico?.Nombre ?? (incidente.GetType().GetProperty("usuarioTecnico")?.GetValue(incidente)?.ToString() ?? "-");
                return View(incidente);
        }

        }



        #endregion

        #region Map helpers

        private IncidenteListado MapToListado(Incidente i)
        {
            if (i == null) return new IncidenteListado();
            string titulo = i.TituloIncidente ?? (i.DescripcionIncidente?.Split('\n').FirstOrDefault()) ?? "-";
            string usuario = i.UsuarioReporta?.Nombre ?? "-";
            string estado = i.EstadoIncidente?.NombreEstado ?? "-";
            string tecnico = i.UsuarioTecnico?.Nombre ?? "-";
     
            return new IncidenteListado
            {
                Codigo_Ticket = i.Id,
                Titulo_Incidente = titulo,
                Usuario_Reporta = usuario,
                Estado = estado,
                Tecnico_Asignado = tecnico
            };
        }

        #endregion

        #region Acciones públicas (vistas)

        public IActionResult MantenerIncidente(int page = 1, int numreg = 10)
        {
            var incidentes = obtenerIncidentes();

            var usuarios = obtenerUsuarios();
            ViewBag.Usuarios = new SelectList(usuarios, "Id", "Nombre");

            var tecnicos = obtenerTecnicos();
            ViewBag.Tecnicos = new SelectList(tecnicos, "Id", "Nombre");

            var estados = obtenerEstados();
            ViewBag.Estados = new SelectList(estados, "Id", "NombreEstado");

            var categorias = obtenerCategorias();
            ViewBag.Categorias = new SelectList(categorias, "Id", "Nombre");

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
            var incidentes = obtenerIncidentes();

            if (usuario > 0)
                incidentes = incidentes.Where(i => (i.UsuarioReporta?.Id ?? 0) == usuario).ToList();
            if (tecnico > 0)
                incidentes = incidentes.Where(i => (i.UsuarioTecnico?.Id ?? 0) == tecnico).ToList();
            if (estado > 0)
                incidentes = incidentes.Where(i => (i.EstadoIncidente?.Id ?? 0) == estado).ToList();
            if (categoria > 0)
                incidentes = incidentes.Where(i => (i.Categoria?.Id ?? 0) == categoria).ToList();

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

            int totalRegistros = incidentes.Count();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / numreg);
            int omitir = numreg * (page - 1);

            ViewBag.totalPaginas = totalPaginas;
            ViewBag.page = page;
            ViewBag.numreg = numreg;

            var resultado = incidentes.Skip(omitir).Take(numreg);
            return View(resultado);
        }

        public IActionResult Create() => View(new Incidente());

        [HttpPost]
        public IActionResult Create(Incidente incidente)
        {
            var nuevo = registrarIncidente(incidente);
            return RedirectToAction("MantenerIncidente");
        }

       

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

        public IActionResult MisIncidentes(bool debug = false)
        {
            var tecnicoId = HttpContext.Session.GetInt32("UsuarioId");
            var tecnicoNombre = HttpContext.Session.GetString("UsuarioNombre")?.Trim();
            if (tecnicoId == null)
                return RedirectToAction("Login", "Usuarios");

            var listado = new List<IncidenteListado>();
            string requestUrl = $"{BaseUrl}/incidentes/tecnico/{tecnicoId.Value}";

            try
            {
                using (var clienteHTTP = new HttpClient())
                {
                    TempData["MisIncidentes_RequestUrl"] = requestUrl;

                    var mensaje = clienteHTTP.GetAsync(requestUrl).Result;
                    var raw = mensaje.Content.ReadAsStringAsync().Result ?? "";

                    TempData["MisIncidentes_ApiStatus"] = $"{(int)mensaje.StatusCode} {mensaje.ReasonPhrase}";
                    TempData["MisIncidentes_RawJsonPreview"] = raw.Length > 2000 ? raw.Substring(0, 2000) + "..." : raw;

                    JArray arr = null;
                    if (mensaje.IsSuccessStatusCode)
                    {
                        try
                        {
                            var root = JToken.Parse(string.IsNullOrWhiteSpace(raw) ? "[]" : raw);
                            if (root.Type == JTokenType.Array) arr = (JArray)root;
                            else if (root.Type == JTokenType.Object)
                            {
                                var obj = (JObject)root;
                                var candidate = obj.Properties().Select(p => p.Value).FirstOrDefault(v => v.Type == JTokenType.Array);
                                arr = candidate != null ? (JArray)candidate : new JArray(root);
                            }
                            else arr = new JArray();
                        }
                        catch
                        {
                            arr = new JArray();
                        }
                    }
                    else
                    {
                        arr = new JArray();
                    }

                    if (arr != null && arr.Count > 0)
                    {
                        var matchedById = arr.Where(t => GetTecnicoIdFromToken(t) == tecnicoId.Value).ToList();
                        if (matchedById.Count > 0)
                        {
                            listado = matchedById.Select(TokenToListado).ToList();
                        }
                        else
                        {
                            var matchedByName = arr.Where(t => GetTecnicoNameFromToken(t)?.Equals(tecnicoNombre, StringComparison.OrdinalIgnoreCase) == true).ToList();
                            if (matchedByName.Count > 0)
                                listado = matchedByName.Select(TokenToListado).ToList();
                        }
                    }

                    if (listado.Count == 0) 
                    {
                        var todosUrl = $"{BaseUrl}/incidentes";
                        var todosMsg = clienteHTTP.GetAsync(todosUrl).Result;
                        var todosRaw = todosMsg.Content.ReadAsStringAsync().Result ?? "[]";
                        TempData["MisIncidentes_AllJsonPreview"] = todosRaw.Length > 2000 ? todosRaw.Substring(0, 2000) + "..." : todosRaw;

                        JArray allArr;
                        try { allArr = JArray.Parse(string.IsNullOrWhiteSpace(todosRaw) ? "[]" : todosRaw); }
                        catch { allArr = new JArray(); }

                        var matchTokens = allArr.Where(t =>
                        {
                            if (GetTecnicoIdFromToken(t) == tecnicoId.Value) return true;
                            var nestedName = GetTecnicoNameFromToken(t);
                            if (!string.IsNullOrWhiteSpace(nestedName) && !string.IsNullOrWhiteSpace(tecnicoNombre)
                                && nestedName.Equals(tecnicoNombre, StringComparison.OrdinalIgnoreCase)) return true;
                            var ut = t["usuarioTecnico"] ?? t["usuario_tecnico"] ?? t["tecnico"];
                            if (ut != null && ut.Type == JTokenType.Object)
                            {
                                var nested = ut["nombre"] ?? ut["Nombre"] ?? ut["nombreTecnico"] ?? ut["nombre_tecnico"];
                                if (nested != null && nested.Type == JTokenType.String &&
                                    nested.ToString().Equals(tecnicoNombre, StringComparison.OrdinalIgnoreCase)) return true;
                            }
                            return false;
                        }).ToList();

                        listado = matchTokens.Select(TokenToListado).ToList();

                        if (listado.Count == 0)
                            TempData["MisIncidentes_Info"] = "No se encontraron incidentes asignados (ni por id ni por nombre). Revisa los previews en TempData.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["MisIncidentes_Error"] = ex.Message;
                System.Diagnostics.Debug.WriteLine("MisIncidentes error: " + ex.ToString());
                listado = new List<IncidenteListado>();
            }

            if (debug)
            {
                return Json(new
                {
                    Session = new { UsuarioId = tecnicoId, UsuarioNombre = tecnicoNombre },
                    RequestUrl = TempData["MisIncidentes_RequestUrl"],
                    ApiStatus = TempData["MisIncidentes_ApiStatus"],
                    RawPreview = TempData["MisIncidentes_RawJsonPreview"],
                    AllPreview = TempData["MisIncidentes_AllJsonPreview"],
                    Info = TempData["MisIncidentes_Info"],
                    Error = TempData["MisIncidentes_Error"],
                    Listado = listado
                });
            }

            return View("MisIncidentes", listado);

            IncidenteListado TokenToListado(JToken token)
            {
                int id = token.Value<int?>("id") ?? token.Value<int?>("Id") ?? 0;

                string titulo = token.SelectToken("tituloIncidente")?.ToString()
                                ?? token.SelectToken("titulo_incidente")?.ToString()
                                ?? token.SelectToken("titulo")?.ToString()
                                ?? "-";

                string usuario = token.SelectToken("usuarioReporta.nombre")?.ToString()
                                 ?? token.SelectToken("usuario_reporta")?.ToString()
                                 ?? token.SelectToken("nombreUsuario")?.ToString()
                                 ?? "-";

                string estado = token.SelectToken("estadoIncidente.nombreEstado")?.ToString()
                                ?? token.SelectToken("nombreEstado")?.ToString()
                                ?? token.SelectToken("estado")?.ToString()
                                ?? "-";

                string tecnico = token.SelectToken("usuarioTecnico.nombre")?.ToString()
                                 ?? token.SelectToken("usuario_tecnico.nombre")?.ToString()
                                 ?? token.SelectToken("nombreTecnico")?.ToString()
                                 ?? token.SelectToken("usuarioTecnico")?.ToString()
                                 ?? "-";

                return new IncidenteListado
                {
                    Codigo_Ticket = id,
                    Titulo_Incidente = titulo,
                    Usuario_Reporta = usuario,
                    Estado = estado,
                    Tecnico_Asignado = tecnico
                };
            }

            int GetTecnicoIdFromToken(JToken token)
            {
                var candidates = new[] { "idUsuarioTecnico", "id_usuario_tecnico", "idTecnico", "id_tecnico", "idUsuario", "id_usuario", "usuarioTecnicoId", "usuario_tecnico_id" };
                foreach (var k in candidates)
                {
                    var t = token[k];
                    if (t != null && t.Type != JTokenType.Null)
                    {
                        if (int.TryParse(t.ToString(), out int v)) return v;
                    }
                }

                var ut = token["usuarioTecnico"] ?? token["usuario_tecnico"] ?? token["tecnico"];
                if (ut != null && ut.Type == JTokenType.Object)
                {
                    var idToken = ut["id"] ?? ut["Id"] ?? ut["ID"];
                    if (idToken != null && int.TryParse(idToken.ToString(), out int v2)) return v2;
                }

                return int.MinValue;
            }

            string GetTecnicoNameFromToken(JToken token)
            {
                var nameCandidates = new[] { "nombreTecnico", "tecnico_nombre", "nombre_tecnico", "nombre", "Nombre", "usuarioTecnico", "usuario_tecnico", "usuarioReporta" };
                foreach (var k in nameCandidates)
                {
                    var t = token[k];
                    if (t == null) continue;
                    if (t.Type == JTokenType.String) return t.ToString();
                    if (t.Type == JTokenType.Object)
                    {
                        var n = t["nombre"] ?? t["Nombre"] ?? t["nombreTecnico"];
                        if (n != null && n.Type == JTokenType.String) return n.ToString();
                    }
        }

                var alt = token.SelectToken("usuarioReporta.nombre") ?? token.SelectToken("usuario_reporta.nombre") ?? token.SelectToken("usuarioReporta");
                if (alt != null && alt.Type == JTokenType.String) return alt.ToString();

                return null;
    }
}


        [HttpGet]
        public IActionResult DebugSession()
        {
            var id = HttpContext.Session.GetInt32("UsuarioId");
            var nombre = HttpContext.Session.GetString("UsuarioNombre");
            var rol = HttpContext.Session.GetString("UsuarioRol");
            return Content($"UsuarioId={id ?? -1}, UsuarioNombre={nombre ?? "null"}, UsuarioRol={rol ?? "null"}");
        }

        #endregion
    }
}
