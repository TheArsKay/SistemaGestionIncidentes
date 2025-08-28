using Microsoft.Data.SqlClient;
using SistemaGestionIncidentesApi.Data.Contrato;
using SistemaGestionIncidentesApi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace SistemaGestionIncidentesApi.Data
{
    public class IncidenteRepositorio : Iincidente
    {
        private readonly string cadenaConexion;

        public IncidenteRepositorio(IConfiguration config)
        {
            cadenaConexion = config["ConnectionStrings:DB"];
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

        public List<Incidente> ListarTodosIncidentes()
        {
            var lista = new List<Incidente>();

            using var cn = new SqlConnection(cadenaConexion);
            cn.Open();

            using var cmd = new SqlCommand(@"
        SELECT 
            id,
            titulo_incidente,
            descripcion_incidente,
            solucion_incidente,
            id_categoria,
            id_estado,
            id_tecnico,
            id_usuario,
            fecha_creacion,
            fecha_modificacion
        FROM Incidente
        ORDER BY fecha_creacion DESC", cn);

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(MapearIncidente(rd));
            }

            return lista;
        }

        public List<Incidente> Listado()
        {
            var lista = new List<Incidente>();
            using var cn = new SqlConnection(cadenaConexion);
            cn.Open();
            using var cmd = new SqlCommand("ListarTodosIncidentes", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(MapearIncidente(rd));
            }
            return lista;
        }

        public List<Incidente> ListarPorTecnico(int tecnicoId)
        {
            var lista = new List<Incidente>();

            using (var cn = new SqlConnection(cadenaConexion)) 
            using (var cmd = new SqlCommand(@"
        SELECT 
            i.id,
            i.titulo_incidente AS tituloIncidente,
            i.descripcion_incidente AS descripcionIncidente,
            i.solucion_incidente AS solucionIncidente,
            i.id_usuario AS idUsuarioReporta,
            u.nombre AS nombreUsuario,
            i.id_categoria AS idCategoria,
            c.nombre_categoria AS nombreCategoria,
            i.id_estado AS idEstadoIncidente,
            e.nombre_estado AS nombreEstadoIncidente,
            i.id_tecnico AS idUsuarioTecnico,
            tec.nombre AS nombreTecnico,
            i.fecha_creacion AS fechaCreacion,
            i.fecha_modificacion AS fechaModificacion
        FROM Incidente i
        LEFT JOIN Usuario u ON u.id = i.id_usuario
        LEFT JOIN Categoria c ON c.id = i.id_categoria
        LEFT JOIN EstadoIncidente e ON e.id = i.id_estado
        LEFT JOIN Usuario tec ON tec.id = i.id_tecnico
        WHERE i.id_tecnico = @tecnicoId
        ORDER BY i.fecha_creacion DESC
    ", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@tecnicoId", tecnicoId);

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(MapearIncidente(rd as SqlDataReader));
                    }
                }
            }

            return lista;
        }

        public Incidente ObtenerPorID(int id)
        {
            Incidente incidente = null;

            using (var cn = new SqlConnection(cadenaConexion))
            using (var cmd = new SqlCommand("ObtenerIncidentePorID", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ID", id);

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        incidente = MapearIncidente(rd as SqlDataReader);
                    }
                }
            }

            return incidente;
        }


        public Incidente Registrar(Incidente incidente)
        {
            using var cn = new SqlConnection(cadenaConexion);
            cn.Open();
            using var cmd = new SqlCommand("RegistrarIncidente", cn); 
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@tituloIncidente", (object)incidente.TituloIncidente ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@descripcionIncidente", (object)incidente.DescripcionIncidente ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@solucionIncidente", (object)incidente.SolucionIncidente ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@idUsuario", incidente.idUsuarioReporta);
            cmd.Parameters.AddWithValue("@idCategoria", incidente.idCategoria);
            cmd.Parameters.AddWithValue("@idEstado", incidente.idEstadoIncidente);
            cmd.Parameters.AddWithValue("@idTecnico", (object)incidente.idUsuarioTecnico ?? DBNull.Value);

   
            var insertedIdObj = cmd.ExecuteScalar();
            if (insertedIdObj != null && int.TryParse(insertedIdObj.ToString(), out var newId))
            {
                return ObtenerPorID(newId);
            }

            return null;
        }

        public Incidente Actualizar(Incidente incidente)
        {
            if (incidente == null || incidente.Id == 0) return null;

            using (var cn = new SqlConnection(cadenaConexion))
            using (var cmd = new SqlCommand(@"
        UPDATE Incidente
        SET 
            titulo_incidente = @titulo,
            descripcion_incidente = @descripcion,
            solucion_incidente = @solucion,
            id_categoria = CASE WHEN @idCategoria > 0 THEN @idCategoria ELSE id_categoria END,
            id_usuario = CASE WHEN @idUsuarioReporta > 0 THEN @idUsuarioReporta ELSE id_usuario END,
            id_tecnico = CASE WHEN @idUsuarioTecnico > 0 THEN @idUsuarioTecnico ELSE id_tecnico END,
            id_estado = CASE WHEN @idEstadoIncidente > 0 THEN @idEstadoIncidente ELSE id_estado END,
            fecha_modificacion = GETDATE()
        WHERE id = @id;
    ", cn))
            {
                cmd.CommandType = CommandType.Text;

               
                cmd.Parameters.AddWithValue("@titulo", (object)incidente.TituloIncidente ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@descripcion", (object)incidente.DescripcionIncidente ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@solucion", (object)incidente.SolucionIncidente ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@idCategoria", incidente.idCategoria);
                cmd.Parameters.AddWithValue("@idUsuarioReporta", incidente.idUsuarioReporta);
                cmd.Parameters.AddWithValue("@idUsuarioTecnico", incidente.idUsuarioTecnico);
                cmd.Parameters.AddWithValue("@idEstadoIncidente", incidente.idEstadoIncidente);

                cmd.Parameters.AddWithValue("@id", incidente.Id);

                cn.Open();
                var filas = cmd.ExecuteNonQuery();
            }

            return ObtenerPorID(incidente.Id);
        }



        public bool Eliminar(int id)
        {
            using var cn = new SqlConnection(cadenaConexion);
            cn.Open();
            using var cmd = new SqlCommand("EliminarIncidente", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", id);
            var afectados = cmd.ExecuteNonQuery();
            return afectados > 0;
        }

        #region Helpers
        private Incidente MapearIncidente(SqlDataReader rd)
        {
            var inc = new Incidente();

            // helper: check columna case-insensitive
            bool ColumnaExiste(string nombre)
            {
                for (int i = 0; i < rd.FieldCount; i++)
                    if (string.Equals(rd.GetName(i), nombre, StringComparison.OrdinalIgnoreCase))
                        return true;
                return false;
            }

            string LeerString(string nombre)
            {
                if (!ColumnaExiste(nombre)) return null;
                var ord = rd.GetOrdinal(nombre);
                return rd.IsDBNull(ord) ? null : rd.GetString(ord);
            }

            int LeerInt(string nombre)
            {
                if (!ColumnaExiste(nombre)) return 0;
                var ord = rd.GetOrdinal(nombre);
                return rd.IsDBNull(ord) ? 0 : rd.GetInt32(ord);
            }

            DateTime? LeerDateTime(string nombre)
            {
                if (!ColumnaExiste(nombre)) return null;
                var ord = rd.GetOrdinal(nombre);
                return rd.IsDBNull(ord) ? (DateTime?)null : rd.GetDateTime(ord);
            }

            // Campos básicos
            inc.Id = LeerInt("id");
            // si tienes codigo_ticket en la tabla, lee y asigna a una propiedad correspondiente en tu modelo Incidente (opcional)
            if (ColumnaExiste("codigo_ticket"))
            {
                // si tu modelo Incidente tiene la propiedad Codigo_Ticket, descomenta la siguiente línea:
                // inc.Codigo_Ticket = LeerInt("codigo_ticket");
            }

            inc.TituloIncidente = LeerString("tituloIncidente") ?? LeerString("titulo_incidente");
            inc.DescripcionIncidente = LeerString("descripcionIncidente") ?? LeerString("descripcion_incidente");
            inc.SolucionIncidente = LeerString("solucionIncidente") ?? LeerString("solucion_incidente");

            inc.FechaCreacion = LeerDateTime("fechaCreacion") ?? inc.FechaCreacion;
            inc.FechaModificacion = LeerDateTime("fechaModificacion") ?? inc.FechaModificacion;

            // IDs
            inc.idUsuarioReporta = LeerInt("idUsuarioReporta") != 0 ? LeerInt("idUsuarioReporta") : LeerInt("id_usuario");
            inc.idCategoria = LeerInt("idCategoria") != 0 ? LeerInt("idCategoria") : LeerInt("id_categoria");
            inc.idEstadoIncidente = LeerInt("idEstadoIncidente") != 0 ? LeerInt("idEstadoIncidente") : LeerInt("id_estado");
            inc.idUsuarioTecnico = LeerInt("idUsuarioTecnico") != 0 ? LeerInt("idUsuarioTecnico") : LeerInt("id_tecnico");

            // Objetos anidados (si los alias están presentes)
            if (ColumnaExiste("nombreUsuario") || ColumnaExiste("usuarioReporta"))
            {
                inc.UsuarioReporta = new Usuario
                {
                    Id = inc.idUsuarioReporta,
                    Nombre = LeerString("nombreUsuario") ?? LeerString("usuarioReporta") ?? LeerString("nombreUsuarioReporta")
                };
            }

            if (ColumnaExiste("nombreCategoria") || ColumnaExiste("categoria"))
            {
                inc.Categoria = new Categoria
                {
                    Id = inc.idCategoria,
                    Nombre = LeerString("nombreCategoria") ?? LeerString("categoria")
                };
            }

            if (ColumnaExiste("nombreEstadoIncidente") || ColumnaExiste("estadoIncidente") || ColumnaExiste("nombre_estado"))
            {
                inc.EstadoIncidente = new EstadoIncidente
                {
                    Id = inc.idEstadoIncidente,
                    NombreEstado = LeerString("nombreEstadoIncidente") ?? LeerString("estadoIncidente") ?? LeerString("nombre_estado")
                };
            }

            if (ColumnaExiste("nombreTecnico") || ColumnaExiste("usuarioTecnico"))
            {
                inc.UsuarioTecnico = new Usuario
                {
                    Id = inc.idUsuarioTecnico,
                    Nombre = LeerString("nombreTecnico") ?? LeerString("usuarioTecnico") ?? LeerString("nombreTecnico")
                };
            }

            return inc;
        }



        #endregion
    }
}
