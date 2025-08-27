using Microsoft.Data.SqlClient;
using SistemaGestionIncidentesApi.Data.Contrato;
using Microsoft.Extensions.Configuration;
using System;

namespace SistemaGestionIncidentesApi.Data
{
    public class NotificacionRepositorio : INotificacionRepositorio
    {
        private readonly string cadenaConexion;

        public NotificacionRepositorio(IConfiguration config)
        {
            cadenaConexion = config["ConnectionStrings:DB"];
        }

        public void CrearNotificacion(int usuarioId, string asunto, string mensaje)
        {
            using var cn = new SqlConnection(cadenaConexion);
            cn.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO Notificaciones (usuario_id, asunto, mensaje, fecha)
                VALUES (@u, @a, @m, GETDATE())", cn);
            cmd.Parameters.AddWithValue("@u", usuarioId);
            cmd.Parameters.AddWithValue("@a", (object)asunto ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@m", (object)mensaje ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
    }
}
