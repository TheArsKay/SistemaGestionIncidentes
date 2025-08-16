using SistemaGestionIncidentesApi.Data.Contrato;
using SistemaGestionIncidentesApi.Models;
using System.Data.SqlClient;

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
            throw new NotImplementedException();
        }

        public bool Eliminar(int id)
        {
            throw new NotImplementedException();
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
                        if(lector != null && lector.HasRows)
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
            Incidente nuevoIncidente = null;
            int nuevoID = 0;

            using (var conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                using (var comando = new SqlCommand("RegistrarIncidente", conexion))
                {
                    comando.CommandType = System.Data.CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@tituloIncidente", incidente.TituloIncidente);
                    comando.Parameters.AddWithValue("@descripcionIncidente", incidente.DescripcionIncidente);
                    comando.Parameters.AddWithValue("@solucionIncidente", incidente.SolucionIncidente );

                    comando.Parameters.AddWithValue("@idUsuario", incidente.IdUsuario);
                    comando.Parameters.AddWithValue("@idCategoria", incidente.IdCategoria);
                    comando.Parameters.AddWithValue("@idEstado", incidente.IdEstado);

                    comando.Parameters.AddWithValue("@idTecnico", incidente.IdTecnico );

                    comando.Parameters.AddWithValue("@fechaCreacion", incidente.FechaCreacion);
                    comando.Parameters.AddWithValue("@fechaModificacion", incidente.FechaModificacion );
                }
            }

            nuevoIncidente = ObtenerPorID(nuevoID);
            return nuevoIncidente;
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

                IdUsuario = reader.GetInt32(reader.GetOrdinal("idUsuario")),
                IdCategoria = reader.GetInt32(reader.GetOrdinal("idCategoria")),
                IdEstado = reader.GetInt32(reader.GetOrdinal("idEstado")),

                IdTecnico = reader.GetInt32(reader.GetOrdinal("idTecnico")),

                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("fechaCreacion")),
                FechaModificacion = reader.GetDateTime(reader.GetOrdinal("fechaModificacion"))
            };
        }
        #endregion
    }
}
