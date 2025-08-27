using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestionIncidentesApi.Models
{
    public class Incidente
    {
        public int Id { get; set; }
        public string Titulo_Incidente { get; set; }
        public string? Descripcion_Incidente { get; set; }   // ← NULL en BD
        public string? Solucion_Incidente { get; set; }      // ← NULL en BD
        public int Id_Usuario { get; set; }
        public int Id_Categoria { get; set; }
        public int Id_Estado { get; set; }
        public int? Id_Tecnico { get; set; }                 // ← NULL en BD
    }
}
