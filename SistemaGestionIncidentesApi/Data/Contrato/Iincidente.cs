using SistemaGestionIncidentesApi.Models;
using SistemaGestionIncidentesApi.Models.DTOs;

namespace SistemaGestionIncidentesApi.Data.Contrato
{
    public interface Iincidente
    {
        List<Incidente> Listado();
        Incidente ObtenerPorID(int id);
        Incidente Registrar(Incidente incidente);
        Incidente Actualizar(Incidente incidente);
        bool Eliminar(int id);
        List<IncidenteListado> ListarIncidentes();


    }
}
