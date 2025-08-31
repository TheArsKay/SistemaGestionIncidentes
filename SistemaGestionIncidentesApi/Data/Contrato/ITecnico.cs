using SistemaGestionIncidentesApi.Models;
using SistemaGestionIncidentesApi.Models.DTOs;
using System.Collections.Generic;

namespace SistemaGestionIncidentesApi.Data.Contrato
{
    public interface ITecnico
    {
        IEnumerable<Tecnico> Listar();
        Tecnico Registrar(Tecnico tecnico);
        Tecnico Actualizar(Tecnico tecnico);
        bool Eliminar(int id);
        IEnumerable<IncidenteTecnicoDTO> ListarIncidentesPorTecnico(int idTecnico);
        bool ActualizarIncidentePorTecnico(int idIncidente, int idEstado, string? solucion);

        IncidenteTecnicoDTO ObtenerIncidentePorTecnico(int idTecnico, int idIncidente);


    }
}
