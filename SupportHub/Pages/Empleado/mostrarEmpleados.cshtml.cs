using Microsoft.AspNetCore.Mvc;
using SupportHub.Helpers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportHub.Modelos;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
namespace SupportHub.Pages.Empleado
{
    [Authorize(Roles = "Administrador,Común")]
    public class mostrarEmpleadosModel : PageModel
    {
        private readonly IConfiguration configuracion;
        private readonly Conexiones conexion;
        public List<Empleados> listaEmpleado = new List<Empleados>();
        public Empleados newEmpleado = new Empleados();
        public String mensajeError = "";
        public List<SelectListItem> Area = new List<SelectListItem>();
        public List<SelectListItem> Cargo = new List<SelectListItem>();

        public mostrarEmpleadosModel(IConfiguration configuration)
        {
            this.configuracion = configuration;
            conexion = new(this.configuracion);
        }

        public void OnGet(string searchQuery = null, bool exito = false, bool intentoRealizado = false, bool esEliminacion = false, bool eliminado = false)
        {
            string cadena = conexion.ObtenerCadenaDisponible();
            if (cadena == null && !string.IsNullOrEmpty(mensajeError))
            {
                // Pasar ErrorMessage a la vista si hay un error de conexión
                ViewData["ErrorMessage"] = mensajeError;
            }

            try
            {
                using (SqlConnection conexion = new SqlConnection(cadena))
                {
                    conexion.Open();
                    SqlCommand comando;
                    if (!string.IsNullOrEmpty(searchQuery))
                    {
                        comando = new SqlCommand("sp_obtener_empleado", conexion);
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@codEmpleado", searchQuery);
                        comando.Parameters.AddWithValue("@nombreEmpleado", searchQuery);
                        comando.Parameters.AddWithValue("@apellidoEmpleado", searchQuery);
                    }
                    else
                    {
                        comando = new SqlCommand("sp_obtener_empleados_general", conexion);
                        comando.CommandType = CommandType.StoredProcedure;
                    }

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            Empleados empleado = new Empleados();

                            empleado.codEmpleado = lector.GetString(0);
                            empleado.nombreEmpleado = lector.GetString(1);
                            empleado.apellidoEmpleado = lector.GetString(2);
                            empleado.telefonoEmpleado = lector.GetString(3);
                            empleado.emailEmpleado = lector.GetString(4);
                            empleado.codArea = lector.GetString(5);
                            empleado.nombreArea = lector.GetString(6);
                            empleado.codCargo = lector.GetString(7);
                            empleado.nombreCargo = lector.GetString(8);
                            empleado.IdCargo = lector.GetInt32(9);
                            empleado.IdArea = lector.GetInt32(10);

                            listaEmpleado.Add(empleado);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                mensajeError = $"No se pudieron obtener empleados de la base: {ex.Message}";
                Console.WriteLine("Error: " + ex.Message);
            }
            // Lista de Area
            try
            {
                using (SqlConnection conexion = new SqlConnection(cadena))
                {
                    conexion.Open();
                    SqlCommand comando = new SqlCommand("select idArea,  codArea  + ' - ' + nombreArea as [Areas] from Areas", conexion);

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            SelectListItem area = new SelectListItem
                            {
                                Value = lector.GetInt32(0).ToString(),
                                Text = lector.GetString(1)
                            };
                            Area.Add(area);
                        }
                    }

                    conexion.Close();
                }
            }
            catch (Exception ex)
            {
                mensajeError = $"No se pudieron obtener áreas de la base: {ex.Message}";
                Console.WriteLine("Error al obtener clientes: " + ex.Message);
            }
            //Lista de  Cargo 
            try
            {
                using (SqlConnection conexion = new SqlConnection(cadena))
                {
                    conexion.Open();
                    SqlCommand comando = new SqlCommand("select idCargo,  codCargo  + ' - ' + nombreCargo as [Cargo] from Cargos", conexion);

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            SelectListItem cargo = new SelectListItem
                            {
                                Value = lector.GetInt32(0).ToString(),
                                Text = lector.GetString(1)
                            };
                            Cargo.Add(cargo);
                        }
                    }

                    conexion.Close();
                }
            }
            catch (Exception ex)
            {
                mensajeError = $"No se pudieron obtener cargos de la base: {ex.Message}";
                Console.WriteLine("Error al obtener clientes: " + ex.Message);
            }
        }

        [TempData]
        public bool exito { get; set; } = false;
        [TempData]
        public bool intentoRealizado { get; set; } = false;
        public bool esEliminacion { get; set; } = false;
        [TempData]
        public bool eliminado { get; set; } = false;
        public int coincidencia { get; set; } = 0;

        public IActionResult OnPost(bool esEliminacion = false)
        {
            this.esEliminacion = esEliminacion;

            if (!esEliminacion)
            {
                newEmpleado.nombreEmpleado = Request.Form["nombre"];
                newEmpleado.apellidoEmpleado = Request.Form["apellido"];
                newEmpleado.telefonoEmpleado = Request.Form["telefono"];
                newEmpleado.emailEmpleado = Request.Form["email"];
                newEmpleado.IdArea = int.Parse(Request.Form["IdArea"]);
                newEmpleado.IdCargo = int.Parse(Request.Form["IdCargo"]);
            }

            string cadena = conexion.ObtenerCadenaDisponible();

            if (esEliminacion)
            {   //solo el administrador puede ejecutar esta acción
                if (User.IsInRole("Administrador"))
                {
                    newEmpleado.codEmpleado = Request.Form["codigo"];
                    int registrosEliminados = 0;
                    try
                    {
                        if (cadena == null && !string.IsNullOrEmpty(mensajeError))
                        {
                            ViewData["ErrorMessage"] = mensajeError;
                            return RedirectToPage("/Empleado/mostrarEmpleados");
                        }

                        using (SqlConnection conexion = new SqlConnection(cadena))
                        {
                            conexion.Open();
                            using (var comando = new SqlCommand("sp_eliminar_empleado", conexion))
                            {
                                comando.CommandType = CommandType.StoredProcedure;
                                comando.Parameters.AddWithValue("@codEmpleado", newEmpleado.codEmpleado);
                                comando.Parameters.AddWithValue("@fecha",DateTime.Now);
                                registrosEliminados = Convert.ToInt32(comando.ExecuteScalar());
                            }
                        }
                        eliminado = true; intentoRealizado = true; exito = true;

                        if (exito)
                        {
                            esEliminacion = false;
                            eliminado = true;
                            return RedirectToPage("/Empleado/mostrarEmpleados");
                        }
                    }
                    catch (Exception ex)
                    {
                        mensajeError = ex.Message;
                        return RedirectToPage("/Empleado/mostrarEmpleados");
                    }
                }
            }
            if (string.IsNullOrEmpty(newEmpleado.nombreEmpleado) ||
                string.IsNullOrEmpty(newEmpleado.emailEmpleado) ||
                string.IsNullOrEmpty(newEmpleado.telefonoEmpleado))
            {
                mensajeError = "Todos los campos son requeridos";
                return RedirectToPage("/Empleado/mostrarEmpleados");
            }

            if (Request.Form["esModificacion"] == "false")
            {
                //Agregar un nuevo empleado
                try
                {
                    // Obtenemos la cadena de conexión
                    if (cadena == null && !string.IsNullOrEmpty(mensajeError))
                    {
                        ViewData["ErrorMessage"] = mensajeError;
                        return RedirectToPage("/Empleado/mostrarEmpleados");
                    }
                    int registrosAgregados = 0;
                    using (SqlConnection conexion = new SqlConnection(cadena))
                    {
                        conexion.Open();
                        string query = "sp_crear_empleado @nombreEmpleado, @apellidoEmpleado, @telefonoEmpleado, @emailEmpleado, @IdCargo, @IdArea";
                        SqlCommand comando = new SqlCommand(query, conexion);

                        // Asignamos los parámetros
                        comando.Parameters.AddWithValue("@nombreEmpleado", newEmpleado.nombreEmpleado);
                        comando.Parameters.AddWithValue("@apellidoEmpleado", newEmpleado.apellidoEmpleado);
                        comando.Parameters.AddWithValue("@telefonoEmpleado", newEmpleado.telefonoEmpleado);
                        comando.Parameters.AddWithValue("@emailEmpleado", newEmpleado.emailEmpleado);
                        comando.Parameters.AddWithValue("@idArea", newEmpleado.IdArea);
                        comando.Parameters.AddWithValue("@idCargo", newEmpleado.IdCargo);

                        registrosAgregados = Convert.ToInt32(comando.ExecuteScalar().ToString());
                    }
                    exito = (registrosAgregados == 1) ? true : false; intentoRealizado = true;
                }
                catch (Exception ex)
                {
                    mensajeError = ex.Message;
                    return RedirectToPage("/Empleado/mostrarEmpleados");
                }
            }
            else
            {
                newEmpleado.codEmpleado = Request.Form["codigo"];
                try
                {
                    if (coincidencia == 0)
                    {
                        //23string cadena = configuracion.GetConnectionString("CadenaConexion");
                        using (SqlConnection conexion = new SqlConnection(cadena))
                        {
                            conexion.Open();

                            using (SqlCommand cmd = new SqlCommand("sp_modificar_empleado", conexion))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                // Agregar parámetros al comando
                                cmd.Parameters.AddWithValue("@codEmpleado", newEmpleado.codEmpleado);
                                cmd.Parameters.AddWithValue("@nombreEmpleado", newEmpleado.nombreEmpleado);
                                cmd.Parameters.AddWithValue("@apellidoEmpleado", newEmpleado.apellidoEmpleado);
                                cmd.Parameters.AddWithValue("@telefono", newEmpleado.telefonoEmpleado);
                                cmd.Parameters.AddWithValue("@emailEmpleado", newEmpleado.emailEmpleado);
                                cmd.Parameters.AddWithValue("@idCargo", newEmpleado.IdCargo);
                                cmd.Parameters.AddWithValue("@idArea", newEmpleado.IdArea);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        exito = true;
                    }
                    else
                    {
                        exito = false;
                        intentoRealizado = true;
                    }
                }
                catch (Exception ex)
                {
                    mensajeError = ex.Message;
                    return RedirectToPage("/Empleado/mostrarEmpleados");
                }
            }

            //ya no es necesario validar si exito es true o false porque de igual manera vamos a redirigirnos a la misma
            // página sin enviar objetos adicionales como new exito = true; porque tempData se encarda de enviar esos datos 
            return RedirectToPage("/Empleado/mostrarEmpleados");
        }
    }
}
