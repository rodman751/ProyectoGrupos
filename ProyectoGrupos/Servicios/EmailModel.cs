namespace ProyectoGrupos.Servicios
{
    public class EmailModel
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; } // Para soportar correos en HTML.
    }
}