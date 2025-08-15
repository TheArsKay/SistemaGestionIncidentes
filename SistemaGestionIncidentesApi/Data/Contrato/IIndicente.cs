using SistemaGestionIncidentesApi.Models;

namespace SistemaGestionIncidentesApi.Data.Contrato
{
    public interface IIndicente
    {
        List<IncidenteListado> ListarIncidentes();
    }
}
