using Microsoft.AspNetCore.Mvc;
using SupportHub.Helpers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportHub.Modelos;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Globalization;
using System.Diagnostics.Eventing.Reader;
using Microsoft.AspNetCore.Identity;

namespace SupportHub.Pages.Ticket
{
    [Authorize]
    public class mostrarTicketsModel : PageModel
    {
        private readonly IConfiguration configuracion;
        private readonly Conexiones conexion;

        public List<Tickets> listaTickets = new();
        public List<Tickets> Estado { get; set; } = new List<Tickets>(); 
        public List<Tickets> Prioridad { get; set; } = new List<Tickets>(); 
        public List<Tickets> ItResponsable { get; set; } = new List<Tickets>(); 
        public Tickets newTickets = new Tickets();

        public Tickets newTicket = new();
        [TempData]
        public bool exito { get; set; } = false;
        [TempData]
        public bool finalizar { get; set; } = false;
        [TempData]
        public bool reabrir { get; set; } = false;
        [TempData]
        public bool comenzar { get; set; } = false;
        [TempData]
        public string errorFechaFinalizacion { get; set; } = string.Empty;

        public mostrarTicketsModel(IConfiguration configuration)
        {
            this.configuracion = configuration;
            conexion = new(this.configuracion);
        }

        public void OnGet(string searchQuery = null, bool exito = false, bool intentoRealizado = false, bool esEliminacion = false, bool eliminado = false)
        {
            string cadena = conexion.ObtenerCadenaDisponible();
            if (string.IsNullOrEmpty(cadena))
            {
                ViewData["ErrorSQL"] = "El sistema no tiene conexión con el servidor. Favor notifique el impase al administrador.";
            }

            try
            {
                using (SqlConnection conexion = new SqlConnection(cadena))
                {
                    conexion.Open();
                    SqlCommand comando;
                    if (!string.IsNullOrEmpty(searchQuery))
                    {
                        comando = new SqlCommand("sp_obtener_ticket_filtrada", conexion);
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@codTicket", searchQuery);
                        comando.Parameters.AddWithValue("@titulo", searchQuery);
                        comando.Parameters.AddWithValue("@nombreEmpleado", searchQuery);
                        comando.Parameters.AddWithValue("@nombreEmpleadoIT", searchQuery);
                    }
                    else
                    {
                        if (User.IsInRole("Técnico"))
                        {
                            string usuario = HttpContext.Session.GetString("usuario");
                            comando = new SqlCommand("sp_obtener_ticket_Por_Técnico_Específico", conexion);
                            comando.CommandType = CommandType.StoredProcedure;
                            comando.Parameters.AddWithValue("@usuario", usuario);
                        }
                        else if (User.IsInRole("Usuario"))
                        {
                            string usuario = HttpContext.Session.GetString("usuario");
                            comando = new SqlCommand("sp_obtener_ticket_Por_Usuario_Creador", conexion);
                            comando.CommandType = CommandType.StoredProcedure;
                            comando.Parameters.AddWithValue("@usuario", usuario);
                        }
                        else
                        {
                            comando = new SqlCommand("sp_obtener_ticket_general", conexion);
                            comando.CommandType = CommandType.StoredProcedure;
                        }
                    }

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            Tickets ticket = new Tickets();
                            ticket.codTicket = !lector.IsDBNull(0) ? lector.GetString(0) : string.Empty;
                            ticket.titulo = !lector.IsDBNull(1) ? lector.GetString(1) : string.Empty;
                            ticket.descripcion = !lector.IsDBNull(2) ? lector.GetString(2) : string.Empty;
                            ticket.imagen = !lector.IsDBNull(3) ? lector.GetString(3) : string.Empty;
                            ticket.fechaCreacion = !lector.IsDBNull(4) ? lector.GetDateTime(4).ToString() : string.Empty;
                            ticket.fechaFinalizado = !lector.IsDBNull(5) ? lector.GetDateTime(5).ToString() : string.Empty;
                            ticket.nombreEmpleado = !lector.IsDBNull(6) ? lector.GetString(6) : string.Empty;
                            ticket.nombreEstado = !lector.IsDBNull(7) ? lector.GetString(7) : string.Empty;
                            ticket.nombrePrioridad = !lector.IsDBNull(8) ? lector.GetString(8) : string.Empty;
                            ticket.nombreEmpleadoIT = !lector.IsDBNull(9) ? lector.GetString(9) : string.Empty;
                            ticket.idEstado = !lector.IsDBNull(10) ? lector.GetInt32(10) : 0;
                            ticket.idPrioridad = !lector.IsDBNull(11) ? lector.GetInt32(11) : 0;
                            ticket.idEmpleado = !lector.IsDBNull(12) ? lector.GetInt32(12) : 0;
                            ticket.idEmpleadoIT = !lector.IsDBNull(13) ? lector.GetInt32(13) : 0;
                            ticket.idTicket = !lector.IsDBNull(14) ? lector.GetInt32(14) : 0;
                            listaTickets.Add(ticket);
                        }
                    }

                    comando = new("select E.idEmpleado, E.codEmpleado + ' - ' + E.nombreEmpleado + ' ' + E.apellidoEmpleado as [empleado] from dbo.Empleados E inner join dbo.Areas A on A.idArea = E.idArea and A.nombreArea = 'Informática'", conexion);
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            var it = new Tickets();
                            it.idEmpleadoIT = lector.GetInt32(0);
                            it.nombreEmpleadoIT = lector.GetString(1);
                            ItResponsable.Add(it);
                        }
                    }

                    comando = new("select * from dbo.estadoTicket where nombreEstado not in ('Cerrado','En Proceso')", conexion);
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            var estado = new Tickets();
                            estado.idEstado = lector.GetInt32(0);
                            estado.nombreEstado = lector.GetString(1);
                            Estado.Add(estado);
                        }
                    }

                    comando = new("select * from dbo.prioridadTicket", conexion);
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            var p = new Tickets();
                            p.idPrioridad = lector.GetInt32(0);
                            p.nombrePrioridad = lector.GetString(1);
                            Prioridad.Add(p);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al obtener tickets: {ex.Message}";
            }
        }

        public IActionResult OnPost()
        {
            string cadena = conexion.ObtenerCadenaDisponible();
            if (string.IsNullOrEmpty(cadena))
            {
                ViewData["ErrorSQL"] = "El sistema no tiene conexión con el servidor. Favor notifique el impase al administrador.";
                return Page();
            }

            int registrosCreados = 0;
            int tkCerradoAgregadas = 0;
            int reabrirTkt = 0;
            int comenzarTkt = 0;
            try
            {
                newTicket.codTicket = Request.Form["codTicket"].ToString();
                if (Request.Form["accion"] == "cerrarTicket")
                {
                    if (User.IsInRole("Administrador") || User.IsInRole("Técnico"))
                    {
                        // Obtener el valor del código del ticket
                        string codigoTicket = Request.Form["codTicket"].ToString();

                        // Obtener la fecha de finalización del formulario
                        string fechaFinalizadoStr = Request.Form["fechaFinalizado"];
                        if (string.IsNullOrEmpty(fechaFinalizadoStr))
                        {
                            ViewData["mensajeError"] = "Debe proporcionar la fecha de finalización.";
                            ViewData["codTicket"] = Request.Form["codTicket"];
                            ViewData["fechaFinalizado"] = fechaFinalizadoStr; // Pasamos la fecha al ViewData
                            return Page();
                        }

                        DateTime fechaCreacionTicket;
                        DateTime fechaFinalizado = DateTime.ParseExact(Request.Form["fechaFinalizado"], "yyyy-MM-dd", CultureInfo.InvariantCulture);

                        using (SqlConnection conexion = new(cadena))
                        {
                            conexion.Open();

                            SqlCommand obtenerFechaCreacionCmd = new SqlCommand("SELECT fechaCreacion FROM Ticket WHERE codTicket = @codTicket", conexion);
                            obtenerFechaCreacionCmd.Parameters.AddWithValue("@codTicket", codigoTicket);

                            object resultado = obtenerFechaCreacionCmd.ExecuteScalar();

                            if (resultado == null)
                            {
                                ViewData["mensajeError"] = "No se encontró la fecha de creación para el ticket especificado.";
                                return Page();
                            }

                            fechaCreacionTicket = Convert.ToDateTime(resultado).Date;

                            // Validar que la fecha de finalización no sea anterior a la fecha de creación
                            if (fechaFinalizado < fechaCreacionTicket)
                            {
                                errorFechaFinalizacion = "La fecha de finalización no puede ser anterior a la fecha de creación del ticket.";
                                return RedirectToPage("/Ticket/mostrarTickets");
                            }

                            SqlCommand comando = new("update Ticket set fechaFinalizado = @fechaFinalizado, idEstado = 3  where codTicket = @codTicket", conexion);

                            comando.Parameters.AddWithValue("@codTicket", newTicket.codTicket);
                            comando.Parameters.AddWithValue("@fechaFinalizado", fechaFinalizado);

                            tkCerradoAgregadas = comando.ExecuteNonQuery();
                        }
                    }
                }
                else if (Request.Form["accion"] == "reabrirTicket")
                {
                    if (User.IsInRole("Administrador"))
                    {
                        using (SqlConnection conexion = new(cadena))
                        {
                            conexion.Open();
                            SqlCommand comando = new("update Ticket set fechaFinalizado = null, idEstado = 2 where codTicket = @codTicket", conexion);

                            comando.Parameters.AddWithValue("@codTicket", newTicket.codTicket);

                            reabrirTkt = comando.ExecuteNonQuery();
                        }
                    }
                }
                else if (Request.Form["accion"] == "comenzarTicket")
                {
                    if (User.IsInRole("Administrador") || User.IsInRole("Técnico"))
                    {
                        using (SqlConnection conexion = new(cadena))
                        {
                            conexion.Open();
                            SqlCommand comando = new("update Ticket set idEstado = 4 where codTicket = @codTicket", conexion);

                            comando.Parameters.AddWithValue("@codTicket", newTicket.codTicket);

                            comenzarTkt = comando.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    newTicket.titulo = Request.Form["tituloTicket"].ToString();
                    newTicket.descripcion = Request.Form["descripcionTicket"].ToString();

                    var imagenFile = Request.Form.Files["imagenTicket"];
                    if (imagenFile != null && imagenFile.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            imagenFile.CopyTo(memoryStream);
                            newTicket.imagen = Convert.ToBase64String(memoryStream.ToArray()); // Almacena la imagen en Base64
                        }
                    }                            
                    else
                    {
                        newTicket.imagen = (Request.Form["imagenBase64"]).ToString() ?? "";
                        LimpiarImagen(newTicket.imagen);
                    }

                    DateTime fechaCreacionTicket = DateTime.Now;
                    string estado = Request.Form["idEstado"];
                    newTicket.idPrioridad = Convert.ToInt32(Request.Form["idPrioridad"]);
                    var it = Request.Form["idItResponsable"];
                    if (!string.IsNullOrEmpty(it))
                    {
                        newTicket.idEmpleadoIT = Convert.ToInt32(Request.Form["idItResponsable"]);
                    }

                    using (SqlConnection conexion = new(cadena))
                    {
                        conexion.Open();

                        if (Request.Form["accion"] == "modificar")
                        {
                            if (User.IsInRole("Administrador"))
                            {   newTicket.idEstado = Convert.ToInt32(Request.Form["idEstado"]);
                                newTicket.idTicket = Convert.ToInt32(Request.Form["idTicket"]);
                                using (SqlCommand comando = new("dbo.sp_modificar_ticket", conexion))
                                {
                                    comando.CommandType = CommandType.StoredProcedure;
                                    comando.Parameters.AddWithValue("@titulo", newTicket.titulo);
                                    comando.Parameters.AddWithValue("@descripcion", newTicket.descripcion);
                                    comando.Parameters.AddWithValue("@imagen", newTicket.imagen ?? "");
                                    comando.Parameters.AddWithValue("@fechaCreacion", fechaCreacionTicket);
                                    comando.Parameters.AddWithValue("@idEstado", newTicket.idEstado);
                                    comando.Parameters.AddWithValue("@idPrioridad", newTicket.idPrioridad);
                                    if (newTicket.idEmpleadoIT != 0)
                                    {
                                        comando.Parameters.AddWithValue("@idEmpleadoIT", newTicket.idEmpleadoIT);
                                    }
                                    comando.Parameters.AddWithValue("@idTicket", newTicket.idTicket);
                                    registrosCreados = Convert.ToInt32(comando.ExecuteScalar().ToString());
                                }
                            }
                        }
                        else
                        {
                            if (User.IsInRole("Administrador") || User.IsInRole("Usuario"))
                            {
                                using (SqlCommand comando = new("dbo.sp_crear_ticket", conexion))
                                {
                                    string usuario = HttpContext.Session.GetString("usuario");
                                    comando.CommandType = CommandType.StoredProcedure;
                                    comando.Parameters.AddWithValue("@titulo", newTicket.titulo);
                                    comando.Parameters.AddWithValue("@descripcion", newTicket.descripcion);
                                    comando.Parameters.AddWithValue("@imagen", newTicket.imagen);
                                    comando.Parameters.AddWithValue("@fechaCreacion", fechaCreacionTicket);
                                    comando.Parameters.AddWithValue("@usuario", usuario);
                                    
                                    comando.Parameters.AddWithValue("@idPrioridad", newTicket.idPrioridad);
                                    if (!string.IsNullOrEmpty(it))
                                    {
                                        comando.Parameters.AddWithValue("@idEmpleadoIT", newTicket.idEmpleadoIT);
                                    }
                                    if (string.IsNullOrEmpty(estado))
                                    {
                                        comando.Parameters.AddWithValue("@idEstado", 2);
                                    } else
                                    {
                                        newTicket.idEstado = Convert.ToInt32(Request.Form["idEstado"]);
                                        comando.Parameters.AddWithValue("@idEstado", newTicket.idEstado);
                                    }

                                    registrosCreados = Convert.ToInt32(comando.ExecuteScalar().ToString());
                                }
                            }
                        }
                    }
                }
                exito = registrosCreados == 1;
                finalizar = tkCerradoAgregadas == 1;
                reabrir = reabrirTkt == 1;
                comenzar = comenzarTkt == 1;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ocurrió el siguiente error en la BD al ejecutar la transacción: {ex.Message}.";
                return RedirectToPage("/Ticket/mostrarTickets");
            }
            if (registrosCreados == 1 || tkCerradoAgregadas == 1 || reabrirTkt == 1 || comenzarTkt == 1)
            {
                return RedirectToPage("/Ticket/mostrarTickets");
            }
            else
            {
                TempData["mensajeError"] = "No se pudo ejecutar la transacción exitosamente.";
                return RedirectToPage("/Ticket/mostrarTickets");
            }
        }

        public void LimpiarImagen(string imagen)
        {
            string Format1 = "data:image/jpeg;base64,";
            if (imagen.Contains(Format1))
            {
                newTicket.imagen = imagen.Replace(Format1, "");
            }
        }
    }
}
