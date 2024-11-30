using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportHub.Modelos;
using SupportHub.Helpers;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
namespace SupportHub.Pages.Proveedor
{
    [Authorize(Roles = "Administrador,Común,Técnico")]
    public class mostrarProveedorModel : PageModel
    {
        private readonly IConfiguration configuracion;
        private readonly Conexiones conexion;
        public List<Proveedores> listaProveedores = new List<Proveedores>();
        public Proveedores newProveedor = new Proveedores();
        public String mensajeError = "";
        public String mensajeExito = "";

        public mostrarProveedorModel(IConfiguration configuration)
        {
            this.configuracion = configuration;
            conexion = new(this.configuracion);
        }

        public void OnGet(string searchQuery = null, bool exito = false, bool intentoRealizado = false, bool esEliminacion = false, bool eliminado = false)
        {
            this.exito = exito;
            this.intentoRealizado = intentoRealizado;
            this.esEliminacion = esEliminacion;
            this.eliminado = eliminado;
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
                        comando = new SqlCommand("sp_obtener_proveedor", conexion);
                        comando.CommandType = System.Data.CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@codProveedor", searchQuery);
                        comando.Parameters.AddWithValue("@nombreProveedor", searchQuery);
                    }
                    else
                    {
                        comando = new SqlCommand("sp_obtener_proveedores_general", conexion);
                        comando.CommandType = System.Data.CommandType.StoredProcedure;
                    }

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            Proveedores proveedor = new Proveedores();
                            proveedor.idProveedor = lector.GetInt32(0);
                            proveedor.codProveedor = lector.GetString(1);
                            proveedor.nombreProveedor = lector.GetString(2);
                            proveedor.direccionProveedor = lector.GetString(3);
                            proveedor.telefonoProveedor = lector.GetString(4);
                            listaProveedores.Add(proveedor);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al obtener proveedores: {ex.Message}";
            }
        }
        /*la propiedad [TempData] almacena temporalmente el valor de la variable durante la redirección. Cuando la vista
        lee el valor de la variable, TempData se encarga de eliminarlo de sí misma, lo que hace que al recargar la página
        la variable vuelva a tomar el valor que le hemos asignado. */
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
            if (User.IsInRole("Administrador") || User.IsInRole("Técnico"))
            {
                this.esEliminacion = esEliminacion;
                //newProveedor.idProveedor = Convert.ToInt32(Request.Form["id"]);
                string cadena = conexion.ObtenerCadenaDisponible();

                if (string.IsNullOrEmpty(cadena))
                {
                    ViewData["ErrorSQL"] = "El sistema no tiene conexión con el servidor. Favor notifique el impase al administrador.";
                    return Page();
                }

                if (esEliminacion)
                {
                    if (User.IsInRole("Administrador"))
                    {
                        newProveedor.idProveedor = Convert.ToInt32(Request.Form["idProveedor"]);
                        int registrosEliminados = 0;
                        try
                        {
                            using (SqlConnection conexion = new SqlConnection(cadena))
                            {
                                conexion.Open();
                                using (var comando = new SqlCommand("sp_eliminar_proveedor", conexion))
                                {
                                    comando.CommandType = CommandType.StoredProcedure;
                                    comando.Parameters.AddWithValue("@idProveedor", newProveedor.idProveedor);
                                    registrosEliminados = Convert.ToInt32(comando.ExecuteScalar());
                                }
                            }
                            exito = registrosEliminados == 1 ? true : false;

                            if (exito)
                            {
                                esEliminacion = false;
                                eliminado = true;
                                return RedirectToPage("/Proveedor/mostrarProveedor");
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["ErrorMessage"] = $"Error al eliminar proveedor: {ex.Message}";
                            return RedirectToPage("/Proveedor/mostrarProveedor");
                        }
                    }
                }

                newProveedor.codProveedor = Request.Form["Codigo"];
                newProveedor.nombreProveedor = Request.Form["nombre"];
                newProveedor.direccionProveedor = Request.Form["direccion"];
                newProveedor.telefonoProveedor = Request.Form["telefono"];

                if (string.IsNullOrEmpty(newProveedor.codProveedor) ||
                    string.IsNullOrEmpty(newProveedor.nombreProveedor) ||
                    string.IsNullOrEmpty(newProveedor.direccionProveedor) ||
                    string.IsNullOrEmpty(newProveedor.telefonoProveedor))
                {
                    TempData["ErrorMessage"] = "Todos los campos son requeridos.";
                    return RedirectToPage("/Proveedor/mostrarProveedor");
                }

                if (Request.Form["esModificacion"] == "false")
                {
                    if (User.IsInRole("Administrador"))
                    {
                        try
                        {
                            int registrosAgregados = 0;
                            using (SqlConnection conexion = new SqlConnection(cadena))
                            {
                                conexion.Open();
                                string query = "dbo.sp_crear_proveedor @codProveedor,@nombreProveedor,@direccionProveedor,@telefonoProveedor";
                                SqlCommand comando = new SqlCommand(query, conexion);

                                comando.Parameters.AddWithValue("@codProveedor", newProveedor.codProveedor);
                                comando.Parameters.AddWithValue("@nombreProveedor", newProveedor.nombreProveedor);
                                comando.Parameters.AddWithValue("@direccionProveedor", newProveedor.direccionProveedor);
                                comando.Parameters.AddWithValue("@telefonoProveedor", newProveedor.telefonoProveedor);

                                registrosAgregados = Convert.ToInt32(comando.ExecuteScalar().ToString());
                            }
                            exito = (registrosAgregados == 1) ? true : false; intentoRealizado = true;
                            return RedirectToPage("/Proveedor/mostrarProveedor");
                        }
                        catch (Exception ex)
                        {
                            TempData["ErrorMessage"] = $"Error al crear proveedor: {ex.Message}";
                            return RedirectToPage("/Proveedor/mostrarProveedor");
                        }
                    }
                }
                else
                {
                    newProveedor.idProveedor = Convert.ToInt32(Request.Form["id"]);
                    try
                    {
                        #region validar coincidencias
                        //voy a obtener algunos campos de los proveedores para hacer algunas comparaciones
                        List<Proveedores> nombreYCodigoProveedores = new List<Proveedores>();

                        using (SqlConnection conexion = new SqlConnection(cadena))
                        {
                            conexion.Open();
                            SqlCommand comando = new SqlCommand("sp_obtener_proveedores_general", conexion);
                            comando.CommandType = System.Data.CommandType.StoredProcedure;

                            using (SqlDataReader lector = comando.ExecuteReader())
                            {
                                while (lector.Read())
                                {
                                    Proveedores proveedor = new Proveedores();
                                    proveedor.codProveedor = lector.GetString(1);
                                    proveedor.nombreProveedor = lector.GetString(2);

                                    nombreYCodigoProveedores.Add(proveedor);
                                }
                            }
                        }
                        //recorriendo la lista para ver si el nuevo nombre que estamos asignando al proveedor ya está 
                        //asignado a alguien más 
                        foreach (var i in nombreYCodigoProveedores)
                        {
                            //si tienen el mismo código y nombre, significa que quiere cambiar un campo distinto al nombre
                            if (i.codProveedor == newProveedor.codProveedor && i.nombreProveedor == newProveedor.nombreProveedor)
                            {
                                break;
                            }//si tienen distinto código y mismo nombre significa que quiere asignar un nombre que ya está ocupado
                            else if (i.codProveedor != newProveedor.codProveedor && i.nombreProveedor == newProveedor.nombreProveedor)
                            {
                                coincidencia += 1;
                                break;
                            }
                        }

                        #endregion

                        if (coincidencia == 0)
                        {
                            //string cadena = configuracion.GetConnectionString("CadenaConexion");
                            using (SqlConnection conexion = new SqlConnection(cadena))
                            {
                                conexion.Open();
                                string query = "dbo.sp_modificar_proveedor @codProveedor,@idProveedor,@nombreProveedor,@direccionProveedor,@telefonoProveedor";
                                SqlCommand comando = new SqlCommand(query, conexion);

                                comando.Parameters.AddWithValue("@idProveedor", newProveedor.idProveedor);
                                comando.Parameters.AddWithValue("@codProveedor", newProveedor.codProveedor);
                                comando.Parameters.AddWithValue("@nombreProveedor", newProveedor.nombreProveedor);
                                comando.Parameters.AddWithValue("@direccionProveedor", newProveedor.direccionProveedor);
                                comando.Parameters.AddWithValue("@telefonoProveedor", newProveedor.telefonoProveedor);

                                comando.ExecuteNonQuery();
                            }
                            exito = true;
                            return RedirectToPage("/Proveedor/mostrarProveedor");
                        }
                        else
                        {
                            exito = false;
                            intentoRealizado = true;
                            return RedirectToPage("/Proveedor/mostrarProveedor");
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = $"Error al modificar proveedor: {ex.Message}";
                        return RedirectToPage("/Proveedor/mostrarProveedor");
                    }
                }
                return RedirectToPage("/Proveedor/mostrarProveedor");
            }
            else
            {
                TempData["ErrorMessage"] = "No tiene el rol requerido para ejecutar esta acción.";
                return RedirectToPage("/Proveedor/mostrarProveedor");
            }
        }
    }
}