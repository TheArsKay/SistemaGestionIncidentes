using System;

namespace SistemaGestionIncidentesApi.Data.Contrato
{
    public interface INotificacionRepositorio
    {
        void CrearNotificacion(int usuarioId, string asunto, string mensaje);
    }
}
