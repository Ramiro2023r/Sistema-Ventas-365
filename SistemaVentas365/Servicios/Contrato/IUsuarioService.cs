using Microsoft.EntityFrameworkCore;
using SistemaVentas365.Models;

namespace SistemaVentas365.Servicios.Contrato
{
    public interface IUsuarioService
    {
        Task<Usuario> GetUsuario(string correo, string clave);
        Task<Usuario> SaveUsuario(Usuario modelo);

    }
}
