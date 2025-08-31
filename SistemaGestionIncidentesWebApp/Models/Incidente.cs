using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestionIncidentesWebApp.Models
{
    public class Incidente
    {
        public int Id { get; set; }

        public string Titulo_Incidente { get; set; }
        public string? Descripcion_Incidente { get; set; }
        public string? Solucion_Incidente { get; set; }

        // FK: estos se llenan en los combos
        [Display(Name = "Usuario")]
        public int Id_Usuario { get; set; }

        [Display(Name = "Categoría")]
        public int Id_Categoria { get; set; }

        [Display(Name = "Estado")]
        public int Id_Estado { get; set; }

        [Display(Name = "Técnico")]
        public int? Id_Tecnico { get; set; }
    }
}
