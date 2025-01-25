using System.ComponentModel.DataAnnotations;

namespace Entidades
{
    public class Actividad
    {
        [Key]
        public int IdActividad { get; set; }

        public int IdGrupo { get; set; }

        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public DateTime FechaActividad { get; set; }

        public int CreadoPor { get; set; }

        public DateTime FechaCreacion { get; set; }

        // Relación con Grupo

        public Grupo Grupo { get; set; }
    }
}
