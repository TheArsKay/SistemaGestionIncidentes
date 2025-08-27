using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestionIncidentesWebApp.Models
{
    public class Incidente
    {
        public int Id { get; set; }

        [Display(Name = "Titulo")]
        public string TituloIncidente { get; set; }

        [Display(Name = "Descripción")]
        public string DescripcionIncidente { get; set; }

        [Display(Name = "Solución")]
        public string SolucionIncidente { get; set; }

  
        public virtual Usuario UsuarioReporta { get; set; }


        public virtual Categoria Categoria { get; set; }
 
        public virtual EstadoIncidente EstadoIncidente { get; set; }

        public virtual Usuario? UsuarioTecnico { get; set; }

  
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaModificacion { get; set; }
    }
}
