using System.ComponentModel.DataAnnotations;

namespace SistemaGestionIncidentesWebApp.Models
{
    public class EstadoIncidente
    {
        public int Id { get; set; }

        [Display(Name = "Estado")]
        public string NombreEstado { get; set; }

        public string Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
