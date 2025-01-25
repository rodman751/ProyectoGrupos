namespace ProyectoGrupos.Models
{
    public class CalendarEventViewModel
    {
        public int IdActividad { get; set; }
        public int IdGrupo { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaActividad { get; set; }
        public string Color { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool EsAdministrador { get; set; }
    }

    public class CalendarViewModel
    {
        public int GrupoId { get; set; }
        public string GrupoNombre { get; set; }
        public List<CalendarEventViewModel> Eventos { get; set; }
    }
}
