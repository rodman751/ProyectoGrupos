using System.ComponentModel.DataAnnotations;

namespace Entidades
{
    public class Usuario
    {
        [Key]

        public int IdUsuario { get; set; }

        public string Nombre { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        public string Contraseña { get; set; }
        public string Rol { get; set; } = "Usuario";

        // Relación con GruposIntegrantes
        public ICollection<GrupoIntegrante> GruposIntegrantes { get; set; }
    }
}
