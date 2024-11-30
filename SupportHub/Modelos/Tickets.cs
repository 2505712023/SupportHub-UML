namespace SupportHub.Modelos
{
    public class Tickets
    {
        public int idTicket { get; set; }
        public string codTicket { get; set; }
        public string titulo { get; set; }
        public string descripcion { get; set; }
        public string imagen { get; set; }
        public string fechaCreacion { get; set; }
        public string fechaFinalizado { get; set; }
        public int idEmpleado { get; set; }
        public string nombreEmpleado { get; set; }
        public int idEstado { get; set; }
        public string nombreEstado { get; set; }
        public int idPrioridad { get; set; }
        public string nombrePrioridad { get; set; }
        public int idEmpleadoIT { get; set; }
        public string nombreEmpleadoIT { get; set; }
    }
}
