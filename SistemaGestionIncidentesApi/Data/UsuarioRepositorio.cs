using BCrypt.Net;
using Microsoft.Data.SqlClient;
using SistemaGestionIncidentesApi.Data.Contrato;
using SistemaGestionIncidentesApi.Models;
using System.Data;

namespace SistemaGestionIncidentesApi.Data
{
    public class UsuarioRepositorio : IUsuario
    {
        private readonly string cadenaConexion;
        private readonly IConfiguration _config;

        public UsuarioRepositorio(IConfiguration config)
        {
            _config = config;
            cadenaConexion = _config["ConnectionStrings:DB"];
        }

        public Usuario IniciarSesion(string email, string clave)
        {
            Usuario usuario = null;

#pragma warning disable CS0618 
            using (var conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                using (var comando = new SqlCommand("ObtenerUsuarioPorEmail", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@correo", email);

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                        
                            string claveHash = reader.GetString(reader.GetOrdinal("clave"));

                            if (BCrypt.Net.BCrypt.Verify(clave, claveHash))
                            {
                                usuario = new Usuario
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                                    Email = reader.GetString(reader.GetOrdinal("email")),
                                    Clave = reader.GetString(reader.GetOrdinal("clave")),
                                    RolId = reader.GetInt32(reader.GetOrdinal("rol_id")),
                                    nombreRol = reader.GetString(reader.GetOrdinal("nombreRol"))
                                };
                            }
                        }
                    }
                }
            }
#pragma warning restore CS0618 
            return usuario; 
        }

        public List<Usuario> Listado()
        {
            var listado = new List<Usuario>();
            using (var conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                using (var comando = new SqlCommand("ListarUsuarios", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listado.Add(new Usuario()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                                Clave = reader.GetString(reader.GetOrdinal("clave")),
                                Email = reader.GetString(reader.GetOrdinal("email")),
                                RolId = reader.GetInt32(reader.GetOrdinal("rol_id")),
                                nombreRol = reader.GetString(reader.GetOrdinal("nombreRol"))
                            });
                        }
                    }
                }
            }
            return listado;
        }

        public Usuario ObtenerPorID(int id)
        {
            Usuario usuario = null;
            using (var conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                using (var comando = new SqlCommand("ObtenerUsuarioPorID", conexion))
                {
                    comando.CommandType = System.Data.CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@ID", id);
                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader != null && reader.HasRows)
                        {
                            reader.Read();
                            usuario = new Usuario()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                                Clave = reader.GetString(reader.GetOrdinal("clave")),
                                Email = reader.GetString(reader.GetOrdinal("email")),
                                RolId = reader.GetInt32(reader.GetOrdinal("rol_id")),
                                nombreRol = reader.GetString(reader.GetOrdinal("nombreRol"))
                            };
                        }
                    }
                }
            }
            return usuario;
        }


        public Usuario RegistrarUsuario(Usuario usuario)
        {
            Usuario nuevoUsuario = null;
            int nuevoID = 0;

            using (var conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                using (var comando = new SqlCommand("RegistrarUsuario", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;

                    comando.Parameters.AddWithValue("@nombre", usuario.Nombre);
                    comando.Parameters.AddWithValue("@clave", BCrypt.Net.BCrypt.HashPassword(usuario.Clave));
                    comando.Parameters.AddWithValue("@correo", usuario.Email);
                    comando.Parameters.AddWithValue("@rol_id", usuario.RolId);

                    nuevoID = Convert.ToInt32(comando.ExecuteScalar());
                }
            }
            nuevoUsuario = ObtenerPorID(nuevoID);
            return nuevoUsuario;

        }

        public List<Rol> ListarRoles()
        {
            var roles = new List<Rol>();
            using (var cn = new SqlConnection(cadenaConexion))
            {
                cn.Open();
                using (var cmd = new SqlCommand("ListarRoles", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            roles.Add(new Rol
                            {
                                Id = rd.GetInt32(rd.GetOrdinal("id")),
                                Nombre = rd.GetString(rd.GetOrdinal("nombre"))
                            });
                        }
                    }
                }
            }
            return roles;
        }

    }
}