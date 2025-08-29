namespace SistemaGestionIncidentesApi.Models
{
    public class EstadoIncidente
    {
        public int Id { get; set; }
        public string? NombreEstado { get; set; }
        public string Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
