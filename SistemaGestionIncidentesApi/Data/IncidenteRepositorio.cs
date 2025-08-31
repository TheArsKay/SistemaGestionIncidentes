using SistemaGestionIncidentesApi.Data.Contrato;
using SistemaGestionIncidentesApi.Models;
using Microsoft.Data.SqlClient;
using System.Data;
//ABRE LA CONEXION A LA BASE DE DATOS MEDIANTE LA LOGICA Y SE INVOCA A TODOS LOS PROCEDIMIENTO ALMACENADO DEL CRUD
namespace FinancieraAPI.Data
{
    public class IncidenteRespositorio : Iincidente
    {
        private readonly string cadenaConexion;
        private readonly IConfiguration _config;

        public IncidenteRespositorio(IConfiguration config)
        {
            _config = config;
            cadenaConexion = _config["ConnectionStrings:DB"];
        }

        public Incidente Actualizar(Incidente incidente)
        {
            using (var connection = new SqlConnection(cadenaConexion))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("ActualizarIncidente", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@idIncidente", incidente.Id);
                    command.Parameters.AddWithValue("@tituloIncidente", incidente.TituloIncidente);
                    command.Parameters.AddWithValue("@descripcionIncidente", (object?)incidente.DescripcionIncidente ?? DBNull.Value);
                    command.Parameters.AddWithValue("@solucionIncidente", (object?)incidente.SolucionIncidente ?? DBNull.Value);
                    command.Parameters.AddWithValue("@idUsuario", incidente.idUsuarioReporta);
                    command.Parameters.AddWithValue("@idCategoria", incidente.idCategoria);
                    command.Parameters.AddWithValue("@idEstado", incidente.idEstadoIncidente);
                    command.Parameters.AddWithValue("@idTecnico", (object?)incidente.idUsuarioTecnico ?? DBNull.Value);

                    command.ExecuteNonQuery();
                }

            }
            return incidente;
        }

        public bool Eliminar(int id)
        {
            var exito = false;
            using (var mbmaConexion = new SqlConnection(cadenaConexion))
            {
                mbmaConexion.Open();
                using (var command = new SqlCommand("EliminarIncidente", mbmaConexion))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@id_incidente", id);
                    exito = command.ExecuteNonQuery() > 0;
                }
            }
            return exito;
        }

        public List<Incidente> Listado()
        {
            var listado = new List<Incidente>();
            using (var conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                using (var comando = new SqlCommand("ListarIncidente", conexion))
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

        public Incidente ObtenerPorID(int id)
        {
            Incidente incidente = null;
            using (var conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                using (var comando = new SqlCommand("ObtenerIncidentePorID", conexion))
                {
                    comando.CommandType = System.Data.CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@ID", id);
                    using (var lector = comando.ExecuteReader())
                    {
                        if (lector != null && lector.HasRows)
                        {
                            lector.Read();
                            incidente = ConvertirReaderEnObjeto(lector);
                        }
                    }
                }
            }
            return incidente;
        }

        public Incidente Registrar(Incidente incidente)
        {
            Incidente nuevoIncidente = new Incidente();
            int nuevoID = 0;

            using (var conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                using (var comando = new SqlCommand("RegistrarIncidente", conexion))
                {
                    comando.CommandType = System.Data.CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@tituloIncidente", incidente.TituloIncidente);
                    comando.Parameters.AddWithValue("@descripcionIncidente", incidente.DescripcionIncidente);
                    comando.Parameters.AddWithValue("@solucionIncidente", incidente.SolucionIncidente);

                    comando.Parameters.AddWithValue("@idUsuario", incidente.idUsuarioReporta);
                    comando.Parameters.AddWithValue("@idCategoria", incidente.idCategoria);
                    comando.Parameters.AddWithValue("@idEstado", incidente.idEstadoIncidente);

                    comando.Parameters.AddWithValue("@idTecnico", incidente.idUsuarioTecnico);

                    nuevoID = Convert.ToInt32(comando.ExecuteScalar());

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


        #region . MÉTODOS PRIVADOS .
        private Incidente ConvertirReaderEnObjeto(SqlDataReader reader)
        {
            return new Incidente()
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                TituloIncidente = reader.GetString(reader.GetOrdinal("tituloIncidente")),
                DescripcionIncidente = reader.GetString(reader.GetOrdinal("descripcionIncidente")),
                SolucionIncidente = reader.GetString(reader.GetOrdinal("solucionIncidente")),
                Categoria = new Categoria()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("idCategoria")),
                    Nombre = reader.GetString(reader.GetOrdinal("nombreCategoria"))
                },
                idCategoria =  reader.GetInt32(reader.GetOrdinal("idCategoria")),
                UsuarioReporta = new Usuario()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("idUsuario")),
                    Nombre = reader.GetString(reader.GetOrdinal("nombreUsuario"))

                },
                idUsuarioReporta = reader.GetInt32(reader.GetOrdinal("idUsuario")),
                UsuarioTecnico = new Tecnico()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("idTecnico")),
                    Nombre = reader.GetString(reader.GetOrdinal("nombreTecnico"))

                },
                idUsuarioTecnico = reader.GetInt32(reader.GetOrdinal("idTecnico")),
                EstadoIncidente = new EstadoIncidente()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("idEstadoIncidente")),
                    NombreEstado = reader.GetString(reader.GetOrdinal("nombreEstadoIncidente"))
                },
                idEstadoIncidente = reader.GetInt32(reader.GetOrdinal("idEstadoIncidente")),
                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("fechaCreacion")),

                FechaModificacion = reader.IsDBNull(reader.GetOrdinal("fechaModificacion"))
                                    ? (DateTime?)null
                                    : reader.GetDateTime(reader.GetOrdinal("fechaModificacion"))
            };
        }

        public List<Incidente> ListarPorTecnico(int tecnicoId)
        {
            throw new NotImplementedException();
        }

        public List<Incidente> ListarTodosIncidentes()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}