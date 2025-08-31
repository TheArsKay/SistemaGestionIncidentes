namespace SistemaGestionIncidentesApi.Models.DTOs
{
    public class IncidenteTecnicoDTO
    {
        public int Id { get; set; }
        public string TituloIncidente { get; set; }
        public string DescripcionIncidente { get; set; }
        public string? SolucionIncidente { get; set; }   // ✅ corregido

        public int IdUsuario { get; set; }
        public string UsuarioNombre { get; set; }

        public int IdCategoria { get; set; }
        public string CategoriaNombre { get; set; }

        public int IdEstado { get; set; }                // ✅ corregido
        public string EstadoNombre { get; set; }

        public int IdTecnico { get; set; }
        public string TecnicoNombre { get; set; }
    }

}
