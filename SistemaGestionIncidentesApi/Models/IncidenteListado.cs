namespace SistemaGestionIncidentesWebApp.Models
{
    public class IncidenteListado
    {
        public int Codigo_Ticket { get; set; }
        public string Titulo_Incidente { get; set; }
        public string Usuario_Reporta { get; set; }
        public string Estado { get; set; }
        public string Tecnico_Asignado { get; set; }
    }
}
