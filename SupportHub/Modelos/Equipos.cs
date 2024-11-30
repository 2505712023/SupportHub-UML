namespace SupportHub.Modelos
{
    public class Equipos
    {
        public string codEquipo { get; set; }
        public string tipoEquipo { get; set; }
        public string marcaEquipo { get; set; }
        public string modeloEquipo { get; set; }
        public int cantidadAdquirida { get; set; }
        public int cantidadDisponible { get; set; }
        public decimal precioEquipo { get; set; }
        public int idProveedor { get; set; }
        public string nombreproveedor { get; set; }
        public string descripcionEquipo { get; set; }
    }
}
