using Microsoft.EntityFrameworkCore;

namespace ProyectoGrupos.Servicios
{
    public class GrupoInactivoTestService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public GrupoInactivoTestService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();

                    // Llama al procedimiento almacenado para pruebas
                    //await dbContext.Database.ExecuteSqlRawAsync("EXEC dbo.MarcarGruposInactivosPruebas");
                    await dbContext.Database.ExecuteSqlRawAsync("EXEC dbo.MarcarGruposInactivos");

                }

                // Ejecuta cada 2 minutos
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }

}
