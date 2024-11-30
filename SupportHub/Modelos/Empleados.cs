using System.ComponentModel.DataAnnotations;

namespace SupportHub.Modelos
{
    public class Empleados
    {

        public int idEmpleado { get; set; }
        public string codEmpleado { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        public string nombreEmpleado { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        public string apellidoEmpleado { get; set; }
        [Required(ErrorMessage = "El teléfono es requerido")]
        [Phone(ErrorMessage = "Número de teléfono inválido")]
        public string telefonoEmpleado { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string emailEmpleado { get; set; }
        [Required(ErrorMessage = "El área es requerida")]
        public int IdArea { get; set; }
        public string codArea { get; set; }
        public string nombreArea { get; set; }
        [Required(ErrorMessage = "El cargo es requerido")]
        public int IdCargo { get; set; }
        public string codCargo { get; set; }
        public string nombreCargo { get; set; } 
    }
}

