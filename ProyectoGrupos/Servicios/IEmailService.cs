namespace ProyectoGrupos.Servicios
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailModel email);
    }
}
