using Microsoft.Data.SqlClient;
using SistemaGestionIncidentesApi.Data.Contrato;
using SistemaGestionIncidentesApi.Models;
using SistemaGestionIncidentesApi.Models.DTOs;
using System.Collections.Generic;
using System.Data;

namespace SistemaGestionIncidentesApi.Data
{
    public class TecnicoRepositorio : ITecnico
    {
        private readonly string _connectionString;

        public TecnicoRepositorio(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DB");
        }

        public IEnumerable<Tecnico> Listar()
        {
            var lista = new List<Tecnico>();
            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand("ListarTecnicos", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Tecnico
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Correo = reader.IsDBNull(2) ? "" : reader.GetString(2)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public Tecnico Registrar(Tecnico tecnico)
        {
            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand("RegistrarTecnico", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nombre", tecnico.Nombre);
                    cmd.Parameters.AddWithValue("@clave", tecnico.Clave);
                    cmd.Parameters.AddWithValue("@correo", tecnico.Correo ?? (object)DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
            return tecnico;
        }

        public Tecnico Actualizar(Tecnico tecnico)
        {
            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand("ActualizarTecnico", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", tecnico.Id);
                    cmd.Parameters.AddWithValue("@nombre", tecnico.Nombre);
                    cmd.Parameters.AddWithValue("@correo", tecnico.Correo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@estado", tecnico.Estado);

                    cmd.ExecuteNonQuery();
                }
            }
            return tecnico;
        }

        public bool Eliminar(int id)
        {
            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand("EliminarTecnico", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public IEnumerable<IncidenteTecnicoDTO> ListarIncidentesPorTecnico(int idTecnico)
        {
            var lista = new List<IncidenteTecnicoDTO>();
            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand("ListarIncidentesPorTecnico", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idTecnico", idTecnico);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new IncidenteTecnicoDTO
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                TituloIncidente = reader.GetString(reader.GetOrdinal("titulo_incidente")),
                                DescripcionIncidente = reader.IsDBNull(reader.GetOrdinal("descripcion_incidente")) ? null : reader.GetString(reader.GetOrdinal("descripcion_incidente")),
                                SolucionIncidente = reader.IsDBNull(reader.GetOrdinal("solucion_incidente")) ? null : reader.GetString(reader.GetOrdinal("solucion_incidente")),
                                UsuarioNombre = reader.GetString(reader.GetOrdinal("UsuarioNombre")),
                                CategoriaNombre = reader.GetString(reader.GetOrdinal("CategoriaNombre")),
                                EstadoNombre = reader.GetString(reader.GetOrdinal("EstadoNombre")),
                                TecnicoNombre = reader.IsDBNull(reader.GetOrdinal("TecnicoNombre")) ? "" : reader.GetString(reader.GetOrdinal("TecnicoNombre"))
                            });
                        }
                    }
                }
            }
            return lista;
        }


        public bool ActualizarIncidentePorTecnico(int idIncidente, int idEstado, string? solucion)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                using (var cmd = new SqlCommand("ActualizarIncidentePorTecnico", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idIncidente", idIncidente);
                    cmd.Parameters.AddWithValue("@idEstado", idEstado);
                    cmd.Parameters.AddWithValue("@solucionIncidente",
                        string.IsNullOrEmpty(solucion) ? (object)DBNull.Value : solucion);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public IncidenteTecnicoDTO ObtenerIncidentePorTecnico(int idTecnico, int idIncidente)
        {
            IncidenteTecnicoDTO dto = null;

            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                using (var cmd = new SqlCommand("ObtenerIncidentePorTecnico", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idTecnico", idTecnico);
                    cmd.Parameters.AddWithValue("@idIncidente", idIncidente);

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            dto = new IncidenteTecnicoDTO
                            {
                                Id = Convert.ToInt32(dr["id"]),
                                TituloIncidente = dr["titulo_incidente"].ToString(),
                                DescripcionIncidente = dr["descripcion_incidente"].ToString(),
                                SolucionIncidente = dr["solucion_incidente"].ToString(),
                                UsuarioNombre = dr["usuarioNombre"].ToString(),
                                CategoriaNombre = dr["categoriaNombre"].ToString(),
                                EstadoNombre = dr["estadoNombre"].ToString(),
                                TecnicoNombre = dr["tecnicoNombre"].ToString()
                            };
                        }
                    }
                }
            }

            return dto;
        }


    }
}
