using Entidades;

namespace ProyectoGrupos.Models
{
    public class GroupManagementViewModel
    {
        public Grupo Group { get; set; }
        public List<UsuarioGrupoDto> GroupMembers { get; set; }
    }

    public class UsuarioGrupoDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public bool EsAdministrador { get; set; }
    }
}
