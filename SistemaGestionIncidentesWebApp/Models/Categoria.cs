using System.ComponentModel.DataAnnotations;

namespace SistemaGestionIncidentesWebApp.Models
{
    public class Categoria
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [Display(Name = "Nombre de la Categoría")]
        public string NombreCategoria { get; set; }

        public string Estado { get; set; } = "A"; // Activo por defecto
    }
}
