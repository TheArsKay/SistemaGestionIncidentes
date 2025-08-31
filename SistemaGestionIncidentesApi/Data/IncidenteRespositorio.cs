using SistemaGestionIncidentesApi.Data.Contrato;
using SistemaGestionIncidentesApi.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using SistemaGestionIncidentesApi.Models.DTOs;

namespace SistemaGestionIncidentesApi.Data
{
    public class IncidenteRepositorio : Iincidente
    {
        private readonly string cadenaConexion;
        private readonly IConfiguration _config;

        public IncidenteRepositorio(IConfiguration config)
        {
            _config = config;
            cadenaConexion = _config["ConnectionStrings:DB"];
        }
//se abre la conexion a la base de datos y se llama Procedim. Almacenados del crud
        public Incidente Actualizar(Incidente incidente)
        {
            using (var connection = new SqlConnection(cadenaConexion))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("ActualizarIncidente", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@idIncidente", incidente.Id);
                    command.Parameters.AddWithValue("@tituloIncidente", incidente.Titulo_Incidente);
                    command.Parameters.AddWithValue("@descripcionIncidente", incidente.Descripcion_Incidente);
                    command.Parameters.AddWithValue("@solucionIncidente", incidente.Solucion_Incidente);
                    command.Parameters.AddWithValue("@idUsuario", incidente.Id_Usuario);
                    command.Parameters.AddWithValue("@idCategoria", incidente.Id_Categoria);
                    command.Parameters.AddWithValue("@idEstado", incidente.Id_Estado);
                    command.Parameters.AddWithValue("@idTecnico", incidente.Id_Tecnico);

                    command.ExecuteNonQuery();
                }
            }
            return incidente;
        }

        public bool Eliminar(int id)
        {
            var exito = false;
            using (var connection = new SqlConnection(cadenaConexion))
            {
                connection.Open();
                using (var command = new SqlCommand("EliminarIncidente", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@id_incidente", id);
                    exito = command.ExecuteNonQuery() > 0;
                }
            }
            return exito;
        }


        public List<Incidente> Listado()
        {
            var listado = new List<Incidente>();
            using (var connection = new SqlConnection(cadenaConexion))
            {
                connection.Open();
                using (var command = new SqlCommand("ListarIncidente", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (var reader = command.ExecuteReader())
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

        public Incidente ObtenerPorID(int id)
        {
            Incidente incidente = null;
            using (var connection = new SqlConnection(cadenaConexion))
            {
                connection.Open();
                using (var command = new SqlCommand("ObtenerIncidentePorID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ID", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader != null && reader.HasRows)
                        {
                            reader.Read();
                            incidente = ConvertirReaderEnObjeto(reader);
                        }
                    }
                }
            }
            return incidente;
        }

        public Incidente Registrar(Incidente incidente)
        {
            Incidente nuevoIncidente;
            int nuevoID;

            using (var connection = new SqlConnection(cadenaConexion))
            {
                connection.Open();
                using (var command = new SqlCommand("RegistrarIncidente", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@tituloIncidente", incidente.Titulo_Incidente);
                    command.Parameters.AddWithValue("@descripcionIncidente", incidente.Descripcion_Incidente);
                    command.Parameters.AddWithValue("@solucionIncidente", incidente.Solucion_Incidente);
                    command.Parameters.AddWithValue("@idUsuario", incidente.Id_Usuario);
                    command.Parameters.AddWithValue("@idCategoria", incidente.Id_Categoria);
                    command.Parameters.AddWithValue("@idEstado", incidente.Id_Estado);
                    command.Parameters.AddWithValue("@idTecnico", incidente.Id_Tecnico);

                    nuevoID = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            nuevoIncidente = ObtenerPorID(nuevoID);
            return nuevoIncidente;
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





        #region Métodos privados
        private Incidente ConvertirReaderEnObjeto(SqlDataReader reader)
        {
            return new Incidente()
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Titulo_Incidente = reader.GetString(reader.GetOrdinal("tituloIncidente")),
                Descripcion_Incidente = reader.IsDBNull(reader.GetOrdinal("descripcionIncidente"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("descripcionIncidente")),
                Solucion_Incidente = reader.IsDBNull(reader.GetOrdinal("solucionIncidente"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("solucionIncidente")),
                Id_Usuario = reader.GetInt32(reader.GetOrdinal("idUsuario")),
                Id_Categoria = reader.GetInt32(reader.GetOrdinal("idCategoria")),
                Id_Estado = reader.GetInt32(reader.GetOrdinal("idEstadoIncidente")),
                Id_Tecnico = reader.IsDBNull(reader.GetOrdinal("idTecnico"))
                                            ? (int?)null
                                            : reader.GetInt32(reader.GetOrdinal("idTecnico"))
            };
        }

        #endregion
    }
}
