using System.ComponentModel.DataAnnotations;

namespace Entidades
{
    public class Grupo
    {
        [Key]

        public int IdGrupo { get; set; }
        public string Nombre { get; set; }

        public string Descripcion { get; set; }
        public int NumeroMaximoIntegrantes { get; set; }
        public int IdCreador { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int NumeroActualIntegrantes { get; set; }

        // Relación con Actividades
        public ICollection<Actividad> Actividades { get; set; }

        // Relación con GruposIntegrantes
        public ICollection<GrupoIntegrante> GruposIntegrantes { get; set; }
    }
}
