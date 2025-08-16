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

  
        public int IdUsuario { get; set; }
      //  public virtual Usuario UsuarioReporta { get; set; }


        public int IdCategoria { get; set; }
      //  public virtual Categoria Categoria { get; set; }

 
        public int IdEstado { get; set; }
      //  public virtual EstadoIncidente EstadoIncidente { get; set; }

        public int? IdTecnico { get; set; }
      //  public virtual Usuario? UsuarioTecnico { get; set; }

  
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime FechaModificacion { get; set; }
    }
}
