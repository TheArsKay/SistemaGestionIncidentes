using SistemaGestionIncidentesApi.Data.Contrato;
using SistemaGestionIncidentesApi.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FinancieraAPI.Data
{
    public class EstadoIncidenteRepositorio : IEstadoIncidente
    {
        private readonly string cadenaConexion;
        private readonly IConfiguration _config;

        public EstadoIncidenteRepositorio(IConfiguration config)
        {
            _config = config;
            cadenaConexion = _config["ConnectionStrings:DB"];
        }

        public EstadoIncidente Actualizar(EstadoIncidente estadoIncidente)
        {
            using (var connection = new SqlConnection(cadenaConexion))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("ActualizarEstadoIncidente", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@id", estadoIncidente.Id);
                    command.Parameters.AddWithValue("@nombre_estado", estadoIncidente.NombreEstado);
                    command.Parameters.AddWithValue("@estado", estadoIncidente.Estado);


                    command.ExecuteNonQuery();
                }

            }
            return estadoIncidente;
        }

        public bool Eliminar(int id)
        {
            var exito = false;
            using (var mbmaConexion = new SqlConnection(cadenaConexion))
            {
                mbmaConexion.Open();
                using (var command = new SqlCommand("EliminarEstadoIncidente", mbmaConexion))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@id_incidente", id);
                    exito = command.ExecuteNonQuery() > 0;
                }
            }
            return exito;
        }

        public List<EstadoIncidente> Listado()
        {
            var listado = new List<EstadoIncidente>();
            using (var conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                using (var comando = new SqlCommand("ListarEstadoIncidente", conexion))
                {
                    comando.CommandType = System.Data.CommandType.StoredProcedure;
                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listado.Add(ConvertirReaderEnObjeto(reader));
                        }
                    }
                }
            }

            return listado;
        }

        public EstadoIncidente ObtenerPorID(int id)
        {
            EstadoIncidente? EstadoIncidente = null;
            using (var conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                using (var comando = new SqlCommand("ObtenerEstadoIncidentePorID", conexion))
                {
                    comando.CommandType = System.Data.CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@ID", id);
                    using (var lector = comando.ExecuteReader())
                    {
                        if(lector != null && lector.HasRows)
                        {
                            lector.Read();
                            EstadoIncidente = ConvertirReaderEnObjeto(lector);
                        }
                    }
                }
            }
            return EstadoIncidente;
        }

        public EstadoIncidente Registrar(EstadoIncidente estadoIncidente)
        {
            EstadoIncidente nuevoEstadoIncidente = null;
            int nuevoID = 0;

            using (var conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                using (var comando = new SqlCommand("RegistrarEstadoIncidente", conexion))
                {
                    comando.CommandType = System.Data.CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@nombre_estado", estadoIncidente.NombreEstado);
                    comando.Parameters.AddWithValue("@estado", estadoIncidente.Estado);
                    nuevoID = Convert.ToInt32(comando.ExecuteScalar());

                }
            }

            nuevoEstadoIncidente = ObtenerPorID(nuevoID);
            return nuevoEstadoIncidente;
        }


        #region . MÉTODOS PRIVADOS .
        private EstadoIncidente ConvertirReaderEnObjeto(SqlDataReader reader)
        {
            return new EstadoIncidente()
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                NombreEstado = reader.GetString(reader.GetOrdinal("nombreEstado")),
                Estado = reader.GetString(reader.GetOrdinal("estado")),
                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("fechaCreacion")),
                FechaModificacion = reader.IsDBNull(reader.GetOrdinal("fechaModificacion"))
                                    ? (DateTime?)null
                                    : reader.GetDateTime(reader.GetOrdinal("fechaModificacion"))
            };
        }
        #endregion
    }
}
