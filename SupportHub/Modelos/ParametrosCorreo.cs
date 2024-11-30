namespace SupportHub.Modelos
{
    public class ParametrosCorreo
    {
        public string Correo { get; set; }
        public string Asunto { get; set; }
        public string Cuerpo { get; set; }
        public string Host { get; set; }
        public int Puerto  { get; set; }
        public string Contraseña { get; set; }
        public bool SslActivo { get; set; }
    }
}
