using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportHub.Modelos;
using SupportHub.Helpers;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace SupportHub.Pages.Equipo
{
    [Authorize(Roles = "Administrador,Común,Técnico")]
    public class mostrarEquiposModel : PageModel
    {
        private readonly IConfiguration configuracion;
        private readonly Conexiones conexion;
        public List<Equipos> listaEquipos = new List<Equipos>();
        public Equipos newEquipo = new Equipos();
        public String mensajeError = "";
        public String mensajeExito = "";
        public List<SelectListItem> Proveedor = new List<SelectListItem>();
        public mostrarEquiposModel(IConfiguration configuration)
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

                    if (string.IsNullOrEmpty(searchQuery))
                    {
                        comando = new SqlCommand("sp_obtener_equipos", conexion);
                        comando.CommandType = System.Data.CommandType.StoredProcedure;
                    }
                    else
                    {
                        comando = new SqlCommand("sp_obtener_equipo", conexion);
                        comando.CommandType = System.Data.CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@codEquipo", searchQuery);
                        comando.Parameters.AddWithValue("@tipoEquipo", searchQuery);
                        comando.Parameters.AddWithValue("@marcaEquipo", searchQuery);
                        comando.Parameters.AddWithValue("@modeloEquipo", searchQuery);
                    }

                    using (SqlDataReader lector = comando.ExecuteReader()) 
                    {
                        while (lector.Read()) 
                        {
                            Equipos equipo = new Equipos();
                            equipo.codEquipo = lector.GetString(0);
                            equipo.tipoEquipo = lector.GetString(1);
                            equipo.marcaEquipo = lector.GetString(2);
                            equipo.modeloEquipo = lector.GetString(3);
                            equipo.cantidadAdquirida = lector.GetInt32(4);
                            equipo.cantidadDisponible = lector.GetInt32(5);
                            equipo.precioEquipo = lector.GetDecimal(6);
                            equipo.nombreproveedor= lector.GetString(7);
                            equipo.idProveedor=lector.GetInt32(8);
                            equipo.descripcionEquipo = lector.GetString(9);
                            listaEquipos.Add(equipo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al obtener equipos: {ex.Message}";
            }

            try
            {
                using (SqlConnection conexion = new SqlConnection(cadena))
                {
                    conexion.Open();
                    SqlCommand comando = new SqlCommand("select idProveedor,  codProveedor  + ' - ' + nombreProveedor as [Proveedor] from Proveedores", conexion);

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            SelectListItem proveedor = new SelectListItem
                            {
                                Value = lector.GetInt32(0).ToString(),
                                Text = lector.GetString(1)
                            };
                            Proveedor.Add(proveedor);
                        }
                    }

                    conexion.Close();
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
            this.esEliminacion = esEliminacion;
         
            newEquipo.tipoEquipo = Request.Form["tipoequipo"];
            newEquipo.marcaEquipo = Request.Form["marcaequipo"];
            newEquipo.modeloEquipo = Request.Form["modeloequipo"];
            newEquipo.cantidadAdquirida =Convert.ToInt32( Request.Form["cantidadequipo"]);
            newEquipo.precioEquipo = Convert.ToDecimal(Request.Form["precioequipo"]);
            newEquipo.idProveedor = Convert.ToInt32(Request.Form["idProveedor"]);
            newEquipo.descripcionEquipo = Request.Form["descripcionequipo"];
            string cadena = conexion.ObtenerCadenaDisponible();

            if (string.IsNullOrEmpty(cadena))
            {
                ViewData["ErrorSQL"] = "El sistema no tiene conexión con el servidor. Favor notifique el impase al administrador.";
                return Page();
            }

            if (esEliminacion)
            {   //solo el administrador puede ejecutar esta acción

                newEquipo.codEquipo = Request.Form["codigo"];
                int registrosEliminados = 0;
                if (User.IsInRole("Administrador"))
                {
                    newEquipo.codEquipo = Request.Form["codigo"];
                    try
                    {
                        using (SqlConnection conexion = new SqlConnection(cadena))
                        {
                            conexion.Open();
                            using (var comando = new SqlCommand("sp_borrar_equipo", conexion))
                            {
                                comando.CommandType = CommandType.StoredProcedure;
                                comando.Parameters.AddWithValue("@codEquipo", newEquipo.codEquipo);
                                registrosEliminados = Convert.ToInt32(comando.ExecuteScalar());
                            }
                        }
                        eliminado = true; intentoRealizado = true; exito = true;

                        if (exito)
                        {
                            esEliminacion = false;
                            eliminado = true;
                            return RedirectToPage("/Equipo/mostrarEquipos");
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = $"Error al eliminar equipo: {ex.Message}";
                        return RedirectToPage("/Equipo/mostrarEquipos");
                    }
                }
            }

            if (string.IsNullOrEmpty(newEquipo.tipoEquipo) ||
                   string.IsNullOrEmpty(newEquipo.marcaEquipo) ||
                   string.IsNullOrEmpty(newEquipo.modeloEquipo) ||
                  (newEquipo.cantidadAdquirida) <= 0 ||
                  (newEquipo.precioEquipo) <= 0 ||
                   string.IsNullOrEmpty(newEquipo.descripcionEquipo)
                   )
            {
                TempData["ErrorMessage"] = "Todos los campos son requeridos";
                return RedirectToPage("/Equipo/mostrarEquipos");
            }

            if (Request.Form["esModificacion"] == "false")
            {
                //Agregar un nuevo equipo
                try
                {
                    int registrosAgregados = 0;
                    using (SqlConnection conexion = new SqlConnection(cadena))
                    {
                        conexion.Open();
                        string query = "sp_crear_equipo @TipoEquipo, @marcaEquipo, @modeloEquipo, @cantidadEquipo, @precioEquipo, @idProveedor, @descripcionEquipo";
                        SqlCommand comando = new SqlCommand(query, conexion);

                        // Asignamos los parámetros
                        comando.Parameters.AddWithValue("@TipoEquipo", newEquipo.tipoEquipo);
                        comando.Parameters.AddWithValue("@marcaEquipo", newEquipo.marcaEquipo);
                        comando.Parameters.AddWithValue("@modeloEquipo", newEquipo.modeloEquipo);
                        comando.Parameters.AddWithValue("@cantidadEquipo", newEquipo.cantidadAdquirida);
                        comando.Parameters.AddWithValue("@precioEquipo", newEquipo.precioEquipo);
                        comando.Parameters.AddWithValue("@idProveedor", newEquipo.idProveedor);
                        comando.Parameters.AddWithValue("@descripcionEquipo", newEquipo.descripcionEquipo);

                        registrosAgregados = Convert.ToInt32(comando.ExecuteScalar().ToString());
                    }
                    exito = (registrosAgregados == 1) ? true : false; intentoRealizado = true;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error al crear equipo: {ex.Message}";
                    return RedirectToPage("/Equipo/mostrarEquipos");
                }
            }
            else
            {
                newEquipo.codEquipo = Request.Form["codigo"];
                try
                {
                    if (coincidencia == 0)
                    {
                        //23string cadena = configuracion.GetConnectionString("CadenaConexion");
                        using (SqlConnection conexion = new SqlConnection(cadena))
                        {
                            conexion.Open();

                            using (SqlCommand cmd = new SqlCommand("sp_actualizar_equipo", conexion))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                // Agregar parámetros al comando
                                cmd.Parameters.AddWithValue("@codEquipo", newEquipo.codEquipo);
                                cmd.Parameters.AddWithValue("@TipoEquipo", newEquipo.tipoEquipo);
                                cmd.Parameters.AddWithValue("@marcaEquipo", newEquipo.marcaEquipo);
                                cmd.Parameters.AddWithValue("@modeloEquipo", newEquipo.modeloEquipo);
                                cmd.Parameters.AddWithValue("@cantidadEquipo", newEquipo.cantidadAdquirida);
                                cmd.Parameters.AddWithValue("@precioEquipo", newEquipo.precioEquipo);
                                cmd.Parameters.AddWithValue("@idProveedor", newEquipo.idProveedor);
                                cmd.Parameters.AddWithValue("@descripcionEquipo", newEquipo.descripcionEquipo);

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
                    TempData["ErrorMessage"] = $"Error al modificar equipo: {ex.Message}";
                    return RedirectToPage("/Equipo/mostrarEquipos");
                }
            }
            return RedirectToPage("/Equipo/mostrarEquipos");
        }
    }
}
