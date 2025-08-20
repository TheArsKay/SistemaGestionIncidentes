using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestionIncidentesApi.Models
{
    public class Incidente
    {
        public int Id { get; set; }

        public string TituloIncidente { get; set; }

        public string DescripcionIncidente { get; set; }

        public string SolucionIncidente { get; set; }

        public int idUsuarioReporta { get; set; }
        public int idCategoria { get; set; }
        public int idEstadoIncidente { get; set; }
        public int idUsuarioTecnico { get; set; }


        public Usuario? UsuarioReporta { get; set; }


        public Categoria? Categoria { get; set; }
 
        public EstadoIncidente? EstadoIncidente { get; set; }

        public Usuario? UsuarioTecnico { get; set; }

  
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaModificacion { get; set; }
    }
}
