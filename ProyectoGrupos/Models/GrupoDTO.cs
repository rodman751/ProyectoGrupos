namespace ProyectoGrupos.Models
{
    public class GrupoDTO
    {
        public int IdGrupo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int NumeroMaximoIntegrantes { get; set; }
        public int NumeroActualIntegrantes { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Estado { get; set; }

        public bool? EsCreador { get; set; } // Indica si el usuario es el creador
        public bool? EsAdministrador { get; set; } // Indica si el usuario es administrador
    }
}
