using Entidades;
using Microsoft.EntityFrameworkCore;

public class DbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbContext(DbContextOptions<DbContext> options)
        : base(options)
    {
    }

    public DbSet<Entidades.Usuario> Usuarios { get; set; } = default!;
    public DbSet<GrupoIntegrante> GruposIntegrantes { get; set; }
    public DbSet<Grupo> Grupos { get; set; }
    public DbSet<Actividad> Actividades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Relación entre GrupoIntegrante y Grupo
        modelBuilder.Entity<GrupoIntegrante>()
            .HasOne(gi => gi.Grupo)
            .WithMany(g => g.GruposIntegrantes)
            .HasForeignKey(gi => gi.IdGrupo)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación entre GrupoIntegrante y Usuario
        modelBuilder.Entity<GrupoIntegrante>()
            .HasOne(gi => gi.Usuario)
            .WithMany(u => u.GruposIntegrantes)
            .HasForeignKey(gi => gi.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación entre Actividad y Grupo
        modelBuilder.Entity<Actividad>()
            .HasOne(a => a.Grupo)
            .WithMany(g => g.Actividades)
            .HasForeignKey(a => a.IdGrupo)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
