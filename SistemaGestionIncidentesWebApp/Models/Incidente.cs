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


        [Display(Name = "Usuario")]
        public virtual Usuario UsuarioReporta { get; set; }


        [Display(Name = "Categoria")]
        public virtual Categoria Categoria { get; set; }

        [Display(Name = "Estados Incidente")]

        public virtual EstadoIncidente EstadoIncidente { get; set; }


        [Display(Name = "Técnico")]
        public virtual Tecnico? UsuarioTecnico { get; set; }

        public int idUsuarioReporta { get; set; }
        public int idCategoria { get; set; }
        public int idEstadoIncidente { get; set; }
        public int idUsuarioTecnico { get; set; }


        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaModificacion { get; set; }
    }
}
