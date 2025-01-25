using System.ComponentModel.DataAnnotations;

namespace Entidades
{
    public class GrupoIntegrante
    {
        [Key]
        public int IdGrupoIntegrante { get; set; }
        public int IdGrupo { get; set; }

        public int IdUsuario { get; set; }
        public bool AdministrarGrupo { get; set; } = false;

        // Relaciones con Grupo
        public Grupo Grupo { get; set; }
        public Usuario Usuario { get; set; }
    }
}
