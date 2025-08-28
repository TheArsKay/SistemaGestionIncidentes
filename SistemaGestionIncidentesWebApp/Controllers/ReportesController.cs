using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using DinkToPdf;
using DinkToPdf.Contracts;
using Newtonsoft.Json;
using SistemaGestionIncidentesWebApp.Models;

namespace SistemaGestionIncidentesWebApp.Controllers
{
    public class ReportesController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IConverter _converter;

        public ReportesController(IConfiguration config, IConverter converter)
        {
            _config = config;
            _converter = converter;
        }

        #region Helpers (consumo API)

        private string BaseUrl => _config["Services:URL"]?.TrimEnd('/') ?? "";

        private List<Incidente> obtenerIncidentes()
        {
            var lista = new List<Incidente>();
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(BaseUrl);
                var msg = http.GetAsync("incidentes").Result;
                if (!msg.IsSuccessStatusCode) return lista;
                var data = msg.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<Incidente>>(data) ?? new List<Incidente>();
            }
            return lista;
        }

        private List<Usuario> obtenerUsuarios()
        {
            var lista = new List<Usuario>();
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(BaseUrl);
                var msg = http.GetAsync("usuarios").Result;
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
                http.BaseAddress = new Uri(BaseUrl);
                var msg = http.GetAsync("tecnico/listar").Result;
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
                http.BaseAddress = new Uri(BaseUrl);
                var msg = http.GetAsync("estados-incidente").Result;
                if (!msg.IsSuccessStatusCode) return lista;
                var data = msg.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<EstadoIncidente>>(data) ?? new List<EstadoIncidente>();
            }
            return lista;
        }

        private List<Categoria> obtenerCategorias()
        {
            var lista = new List<Categoria>();
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri(BaseUrl);
                var msg = http.GetAsync("categoria/listar").Result;
                if (!msg.IsSuccessStatusCode) return lista;
                var data = msg.Content.ReadAsStringAsync().Result;
                lista = JsonConvert.DeserializeObject<List<Categoria>>(data) ?? new List<Categoria>();
            }
            return lista;
        }

        #endregion

 
        public IActionResult Index(int page = 1, int numreg = 10, int usuario = 0, int tecnico = 0, int estado = 0, int categoria = 0)
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

        [HttpGet]
        public IActionResult ExportarPDF(int usuario = 0, int tecnico = 0, int estado = 0, int categoria = 0)
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

            // catálogos
            var usuarios = obtenerUsuarios();
            var tecnicos = obtenerTecnicos();
            var estados = obtenerEstados();
            var categorias = obtenerCategorias();

            int totalRegistros = incidentes.Count();

            string html = @"
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        table { width:100%; border-collapse: collapse; }
        th, td { border: 1px solid black; padding: 5px; text-align: center; }
        th { background-color: #f2f2f2; }
    </style>
</head>
<body>
    <h2 style='text-align:center;'>Reporte de Incidentes</h2>
    <p>Total registros: " + totalRegistros + @"</p>
    <table>
        <tr>
            <th>ID</th>
            <th>Título</th>
            <th>Usuario</th>
            <th>Técnico</th>
            <th>Estado</th>
            <th>Categoría</th>
        </tr>";

            foreach (var i in incidentes)
            {
                var usuarioNombre = i.UsuarioReporta?.Nombre ?? usuarios.FirstOrDefault(u => u.Id == (i.UsuarioReporta?.Id ?? 0))?.Nombre ?? "-";
                var tecnicoNombre = i.UsuarioTecnico?.Nombre ?? tecnicos.FirstOrDefault(t => t.Id == (i.UsuarioTecnico?.Id ?? 0))?.Nombre ?? "-";
                var estadoNombre = i.EstadoIncidente?.NombreEstado ?? estados.FirstOrDefault(e => e.Id == (i.EstadoIncidente?.Id ?? 0))?.NombreEstado ?? "-";
                var categoriaNombre = i.Categoria?.Nombre ?? categorias.FirstOrDefault(c => c.Id == (i.Categoria?.Id ?? 0))?.Nombre ?? "-";

                html += "<tr>";
                html += $"<td>{i.Id}</td>";
                html += $"<td>{(i.TituloIncidente ?? "-")}</td>";
                html += $"<td>{usuarioNombre}</td>";
                html += $"<td>{tecnicoNombre}</td>";
                html += $"<td>{estadoNombre}</td>";
                html += $"<td>{categoriaNombre}</td>";
                html += "</tr>";
            }

            html += "</table></body></html>";

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Landscape,
                    Margins = new MarginSettings { Top = 10, Bottom = 10 }
                },
                Objects = { new ObjectSettings { HtmlContent = html } }
            };

            var file = _converter.Convert(pdf);
            return File(file, "application/pdf", "ReporteIncidentes.pdf");
        }
    }
}
