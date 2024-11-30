namespace SupportHub.Modelos
{
    public class Entrega
    {
        public int idEntrega { get; set; }
        public string? codEntrega { get; set; }
        public int idTipoEntrega { get; set; }
        public string? nombreTipoEntrega { get; set; }
        public int cantidadEntrega { get; set; }
        public string? fechaEntrega { get; set; }
        public string? fechaDevolucion { get; set; }
        public string? observacionEntrega { get; set; }
        public int idEmpleadoEntrega { get; set; }
        public string? nombreEmpleadoEntrego { get; set; }
        public int idEmpleadoRecibe { get; set; }
        public string? nombreEmpleadoRecibio { get; set; }
        public int idEquipo { get; set; }
        public string? equipo { get; set; }
    }
}
