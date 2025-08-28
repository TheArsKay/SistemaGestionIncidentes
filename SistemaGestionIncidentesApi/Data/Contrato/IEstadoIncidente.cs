using SistemaGestionIncidentesApi.Models;

namespace SistemaGestionIncidentesApi.Data.Contrato
{
    public interface IEstadoIncidente
    {
        List<EstadoIncidente> Listado();
        EstadoIncidente ObtenerPorID(int id);
        EstadoIncidente Registrar(EstadoIncidente estadoIncidente);
        EstadoIncidente Actualizar(EstadoIncidente estadoIncidente);
        bool Eliminar(int id);
    }
}
