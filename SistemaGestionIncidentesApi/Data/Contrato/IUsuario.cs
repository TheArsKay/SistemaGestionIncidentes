using SistemaGestionIncidentesApi.Models;

namespace SistemaGestionIncidentesApi.Data.Contrato
{
    public interface IUsuario
    {
        List<Usuario> Listado();
        Usuario ObtenerPorID(int id);
        Usuario RegistrarUsuario(Usuario usuario);
        Usuario IniciarSesion(string email, string clave);
        List<Rol> ListarRoles();
    }
}
