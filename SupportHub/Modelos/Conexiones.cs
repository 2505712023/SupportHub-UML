using Microsoft.Extensions.Configuration;
using SupportHub.Helpers;

namespace SupportHub.Modelos
{
    public class Conexiones
    {
        private readonly IConfiguration configuration;

        public Conexiones(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string ObtenerCadenaDisponible()
        {
            try
            {
                // Intenta primero con la cadena de conexión principal
                if (PingHelper.PingHost("100.101.36.39"))
                {
                    return configuration.GetConnectionString("CadenaConexion");
                }
                else if (PingHelper.PingHost("25.2.143.28"))
                {
                    return configuration.GetConnectionString("CadenaConexionHamachi");
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
