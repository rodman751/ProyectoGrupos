using Entidades;

namespace ProyectoGrupos.Models
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalGroups { get; set; }
        public int TotalActivities { get; set; }

        public List<GroupStateDistribution> GroupStateDistribution { get; set; }
        public List<UserRoleDistribution> UserRoleDistribution { get; set; }
        public List<GroupMemberTrend> GroupMemberTrends { get; set; }
        public List<Actividad> RecentActivities { get; set; }
    }
    public class GroupStateDistribution
    {
        public string Estado { get; set; }
        public int Count { get; set; }
    }

    public class UserRoleDistribution
    {
        public string Rol { get; set; }
        public int Count { get; set; }
    }

    public class GroupMemberTrend
    {
        public string NombreGrupo { get; set; }
        public int NumeroIntegrantes { get; set; }
    }
}
