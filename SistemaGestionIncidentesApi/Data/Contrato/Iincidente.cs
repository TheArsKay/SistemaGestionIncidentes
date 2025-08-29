using SistemaGestionIncidentesApi.Models;

namespace SistemaGestionIncidentesApi.Data.Contrato
{
    public interface Iincidente
    {
        List<Incidente> Listado();
        List<Incidente> ListarPorTecnico(int tecnicoId);
        List<Incidente> ListarTodosIncidentes();


        Incidente ObtenerPorID(int id);
        Incidente Registrar(Incidente incidente);
        Incidente Actualizar(Incidente incidente);
        bool Eliminar(int id);
        List<IncidenteListado> ListarIncidentes();
    }
}
