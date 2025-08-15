using Microsoft.Data.SqlClient;
using SistemaGestionIncidentesApi.Data.Contrato;
using SistemaGestionIncidentesApi.Models;
using System.Data;

namespace SistemaGestionIncidentesApi.Data
{
    public class IncidenteRepositorio : IIndicente
    {
        private readonly string cadenaConexion;
        private readonly IConfiguration _config;

        public IncidenteRepositorio(IConfiguration config)
        {
            _config = config;
            cadenaConexion = _config["ConnectionStrings:DB"];
        }

        public List<IncidenteListado> ListarIncidentes()
        {
            var lista = new List<IncidenteListado>();

            using (var cn = new SqlConnection(cadenaConexion))
            {
                cn.Open();
                using (var cmd = new SqlCommand("ListarIncidentes", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add(new IncidenteListado
                            {
                                Codigo_Ticket = rd.GetInt32(rd.GetOrdinal("codigo_ticket")),
                                Titulo_Incidente = rd.GetString(rd.GetOrdinal("titulo_incidente")),
                                Usuario_Reporta = rd.GetString(rd.GetOrdinal("usuario_reporta")),
                                Estado = rd.GetString(rd.GetOrdinal("estado")),
                                Tecnico_Asignado = rd.GetString(rd.GetOrdinal("tecnico_asignado"))
                            });
                        }
                    }
                }
            }

            return lista;
        }
    }
}
