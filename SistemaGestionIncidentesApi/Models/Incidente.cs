namespace SistemaGestionIncidentesApi.Models
{
    public class Incidente
    {
        public int Id { get; set; }
        public string Titulo_Incidente { get; set; }
        public string Descripcion_Incidente { get; set; }
        public string? Solucion_Incidente { get; set; }
        public int Id_Usuario { get; set; }
        public int Id_Categoria { get; set; }
        public int Id_Estado { get; set; }
        public int? Id_Tecnico { get; set; }

    }
}
