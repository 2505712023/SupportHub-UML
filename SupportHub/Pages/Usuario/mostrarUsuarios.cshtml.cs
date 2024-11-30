using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportHub.Modelos;
using System.Data.SqlClient;
using System.Data;
using SupportHub.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using Microsoft.Extensions.Caching.Memory;

namespace SupportHub.Pages.Usuario
{
    [Authorize(Roles = "Administrador,Común,Técnico")]
    public class mostrarUsuariosModel : PageModel
    {
        private readonly IConfiguration configuracion;
        private readonly Conexiones conexion;
        private readonly IMemoryCache _cache;
        private const string EmpleadosCacheKey = "listaEmpleados";
        public List<Usuarios> listaUsuarios = new List<Usuarios>();
        public List<Empleados> infoEmpleados = new List<Empleados>();
        public List<Usuarios> Roles { get; set; } = new List<Usuarios>();
        public Usuarios newUsuario = new Usuarios();
        public string mensajeError = "";
        public string mensajeExito = "";

        public mostrarUsuariosModel(IConfiguration configuration, IMemoryCache memoryCache)
        {
            this.configuracion = configuration;
            conexion = new(this.configuracion);
            _cache = memoryCache;
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

                    if (!string.IsNullOrEmpty(searchQuery))
                    {
                        Buscar(BuscarUsuarioEspecífico(searchQuery, conexion));
                    }
                    else
                    {
                        SqlCommand comando = new SqlCommand("sp_obtener_usuarios", conexion);
                        Buscar(comando);
                    }

                    TempData["usuarioEncontrado"] = usuarioEncontrado = listaUsuarios.Count == 0 ? false : true;
                    if (!usuarioEncontrado)
                    {
                        SqlCommand comando = new SqlCommand("sp_obtener_usuarios", conexion);
                        Buscar(comando);
                    }
                    string queryrol = "SELECT nombreRol FROM Roles";

                    using (SqlCommand command = new SqlCommand(queryrol, conexion))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var rol = new Usuarios
                                {
                                    RolUsuario = reader.GetString(0)
                                };

                                Roles.Add(rol);
                            }
                        }
                    }

                    JsonResult OnGetPersonData(string codEmpleado)
                    {
                        string querycodigo = "SELECT nombreEmpleado, apellidoEmpleado FROM Empleado WHERE codEmpleado = @codEmpleado";
                        using (SqlCommand command = new SqlCommand(querycodigo, conexion))
                        {
                            command.Parameters.AddWithValue("@codEmpleado", codEmpleado);

                            conexion.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            string nombre = "";
                            string apellido = "";

                            if (reader.Read())
                            {
                                nombre = reader["nombreEmpleado"].ToString();
                                apellido = reader["apellidoEmpleado"].ToString();
                            }

                            conexion.Close();
                            return new JsonResult(new { nombre, apellido });
                        }
                    }

                    //recuperar el código, nombre y apellidos de los empleados
                    string querycodigo = "SELECT codEmpleado, nombreEmpleado, apellidoEmpleado FROM Empleados e" +
                        " WHERE (e.idEmpleado NOT IN(SELECT idEmpleado FROM Usuarios)) and e.empleadoActivo = 1";

                    using (SqlCommand command = new SqlCommand(querycodigo, conexion))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var codigoempleados = new Empleados
                                {
                                    codEmpleado = reader.GetString(0),
                                    nombreEmpleado = reader.GetString(1),
                                    apellidoEmpleado = reader.GetString(2)
                                };

                                infoEmpleados.Add(codigoempleados);
                            }
                        }
                    }                  
                }
                //guardar la lista infoEmpleados en el chaché del servidor durante 5 minutos
                _cache.Set(EmpleadosCacheKey, infoEmpleados, TimeSpan.FromMinutes(25));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al obtener usuarios: {ex.Message}";
            }
        }

        #region propiedades
        [TempData]
        public bool usuarioEncontrado { get; set; } = true;
        [TempData]
        public bool exito { get; set; } = false;
        [TempData]
        public bool intentoRealizado { get; set; } = false;
        public bool esEliminacion { get; set; } = false;
        [TempData]
        public bool eliminado { get; set; } = false;
        public int coincidencia { get; set; } = 0;
        #endregion

        public IActionResult OnPost(bool esEliminacion=false)
        {
            this.esEliminacion = esEliminacion;
            if (esEliminacion)
            {   //solo el administrador puede ejecutar esta acción
                if (User.IsInRole("Administrador"))
                {
                    newUsuario.LoginUsuario = Request.Form["usuario"];
                    int registrosEliminados = 0;
                    try
                    {
                        string cadena = conexion.ObtenerCadenaDisponible();

                        if (string.IsNullOrEmpty(cadena))
                        {
                            ViewData["ErrorSQL"] = "El sistema no tiene conexión con el servidor. Favor notifique el impase al administrador.";
                            return Page();
                        }

                        using (SqlConnection conexion = new SqlConnection(cadena))
                        {// y si le pones sqlcommand al iniscio?

                            conexion.Open();
                            using (var comando = new SqlCommand("sp_borrar_usuario", conexion))
                            {
                                comando.CommandType = CommandType.StoredProcedure;
                                comando.Parameters.AddWithValue("@loginUsuario", newUsuario.LoginUsuario);
                                registrosEliminados = Convert.ToInt32(comando.ExecuteScalar());
                            }
                        }
                        eliminado = true; intentoRealizado = true; exito = true;

                        if (exito)
                        {
                            esEliminacion = false;
                            eliminado = true;
                            return RedirectToPage("/Usuario/mostrarUsuarios");
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = $"Error al eliminar equipo: {ex.Message}";
                        return RedirectToPage("/Usuario/mostrarUsuarios");
                    }
                }
            }

            int idUsuarioLogueado = Convert.ToInt32(HttpContext.Session.GetInt32("id"));
            //solo permitir al administrador hacer cualquier solicitud post para esta página
            if (User.IsInRole("Administrador"))
            {
                var activo = newUsuario.ActivoUsuario = Request.Form["activo"].Count > 0;
                if (activo)
                {
                    newUsuario.ActivoUsuario = true;
                }
                else newUsuario.ActivoUsuario = false;
                //si se quiere agregar un nuevo usuario
                if (Request.Form["esModificacion"] == "false")
                {
                    newUsuario.RolUsuario = Request.Form["rol"];
                    newUsuario.LoginUsuario = Request.Form["usuario"];
                    newUsuario.CodEmpleado = Request.Form["nombre"];
                    newUsuario.ClaveUsuario = Request.Form["contraseña"];
                    string codEmpleado = newUsuario.CodEmpleado.Substring(0, 7);
                    //aquí va el código para agregar usuarios 
                    try
                    {
                        string cadena = conexion.ObtenerCadenaDisponible();
                        using (SqlConnection conexion = new SqlConnection(cadena))
                        {
                            conexion.Open();
                            using (SqlCommand comando = new("sp_crear_usuario", conexion))
                            {
                                comando.CommandType = CommandType.StoredProcedure;
                                comando.Parameters.AddWithValue("@claveUsuario", newUsuario.ClaveUsuario);
                                comando.Parameters.AddWithValue("@loginUsuario", newUsuario.LoginUsuario);
                                comando.Parameters.AddWithValue("@activoUsuario",newUsuario.ActivoUsuario);
                                comando.Parameters.AddWithValue("@codEmpleado",codEmpleado);
                                comando.Parameters.AddWithValue("@rol",newUsuario.RolUsuario);
                                comando.ExecuteNonQuery();
                                exito = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = $"Error al eliminar equipo: {ex.Message}";
                        return RedirectToPage("/Usuario/mostrarUsuarios");
                    }

                }//si se quiere modificar un usuario
                else if(Request.Form["esModificacion"] == "true")
                {
                    newUsuario.RolUsuario = Request.Form["rol"];
                    newUsuario.LoginUsuario = Request.Form["usuario"];
                    newUsuario.CodEmpleado = Request.Form["nombre"];
                    newUsuario.IDUsuario = Convert.ToInt32(Request.Form["id"]);
                    newUsuario.ClaveUsuario = Request.Form["contraseña"];
                    string codEmpleado = newUsuario.CodEmpleado.Substring(0,7);
                    try
                    {
                        #region validar coincidencias
                        //voy a obtener algunos campos de los usuarios para hacer algunas comparaciones

                        List<Usuarios> nombreUsuarios = new List<Usuarios>();

                        string cadena = conexion.ObtenerCadenaDisponible();
                        using (SqlConnection conexion = new SqlConnection(cadena))
                        {
                            conexion.Open();
                            SqlCommand comando = new SqlCommand("sp_obtener_usuarios", conexion);
                            comando.CommandType = CommandType.StoredProcedure;

                            using (SqlDataReader lector = comando.ExecuteReader())
                            {
                                while (lector.Read())
                                {
                                   Usuarios usuario = new Usuarios();
                                    usuario.LoginUsuario = lector.GetString(2);
                                    usuario.IDEmpleado = lector.GetInt32(5);
                                    nombreUsuarios.Add(usuario);
                                }
                            }
                        }
                        //recorriendo la lista para ver si el nuevo nombre que estamos asignando al proveedor ya está 
                        //asignado a alguien más 
                        foreach (var i in nombreUsuarios)
                        {
                            //si tienen el mismo código y nombre de usuario, significa que quiere cambiar un campo distinto al nombre de usuario
                            if (i.IDEmpleado == newUsuario.IDEmpleado && i.LoginUsuario == newUsuario.LoginUsuario)
                            {
                                break;
                            }//si tienen distinto código y mismo nombre de usuario significa que quiere asignar un nombre de usuario que ya está ocupado
                            else if (i.IDEmpleado != newUsuario.IDEmpleado && i.LoginUsuario == newUsuario.LoginUsuario)
                            {
                                coincidencia += 1;
                                break;
                            }
                        }

                        #endregion

                        if (coincidencia == 0)
                        {
                            using (SqlConnection conexion = new SqlConnection(cadena))
                            {
                                conexion.Open();
                                //este comando es para modificar login, activo, empleado asociado y rol
                                SqlCommand comando = new SqlCommand("sp_modificar_usuario", conexion);

                                comando.CommandType = CommandType.StoredProcedure;
                                comando.Parameters.AddWithValue("@loginUsuario", newUsuario.LoginUsuario);
                                comando.Parameters.AddWithValue("@activoUsuario", newUsuario.ActivoUsuario);
                                comando.Parameters.AddWithValue("@idUsuario", newUsuario.IDUsuario);
                                comando.Parameters.AddWithValue("@codEmpleado", codEmpleado);
                                comando.Parameters.AddWithValue("@rol", newUsuario.RolUsuario);

                                //este comando es para modificar la contraseña
                                SqlCommand cmd = new SqlCommand("sp_modificar_contraseña", conexion);

                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@claveUsuario", newUsuario.ClaveUsuario);
                                cmd.Parameters.AddWithValue("@idUsuario", newUsuario.IDUsuario);
                                cmd.Parameters.AddWithValue("@loginUsuario",newUsuario.LoginUsuario);

                                //si está modificando su misma información lo redirijo al login para poder hacer un mejor seguimiento de su sesión:
                                if (idUsuarioLogueado == newUsuario.IDUsuario) 
                                {
                                    comando.ExecuteNonQuery();
                                    cmd.ExecuteNonQuery();
                                    exito = true;
                                    return RedirectToPage("/Welcome/Logout");
                                }
                                else if(idUsuarioLogueado != newUsuario.IDUsuario)
                                {
                                    comando.ExecuteNonQuery();
                                    cmd.ExecuteNonQuery();
                                    exito = true;
                                }
                            }
                        }
                        else
                        {
                            exito = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = $"Error al eliminar equipo: {ex.Message}";
                        return RedirectToPage("/Usuario/mostrarUsuarios");
                    }
                }
                
                return RedirectToPage("/Usuario/mostrarUsuarios");
            } 
            else { return Page(); }
        }

        public JsonResult OnGetEmpleados(string term)
        {
            if (_cache.TryGetValue(EmpleadosCacheKey, out List<Empleados> infoEmpleados)) { 
                var sugerencias = infoEmpleados.Where(e => e.nombreEmpleado.StartsWith(term, StringComparison.OrdinalIgnoreCase) ||
                e.apellidoEmpleado.StartsWith(term, StringComparison.OrdinalIgnoreCase)).Select(e => new
                {
                NombreCompleto = e.codEmpleado + " " +  e.nombreEmpleado + " " + e.apellidoEmpleado 
                 }).ToList();

                //devolver el nombre completo y el código de empleado en las sugerencias
                return new JsonResult(sugerencias);
            }
            return new JsonResult(new List<Empleados>());
        }
        public SqlCommand BuscarUsuarioEspecífico(string searchQuery, SqlConnection conexion)
        {
            SqlCommand comando = new SqlCommand("sp_obtener_usuario", conexion);
            comando.CommandType = CommandType.StoredProcedure;
            comando.Parameters.AddWithValue("@LoginUsuario", searchQuery);
            comando.Parameters.AddWithValue("@codEmpleado", searchQuery);
            comando.Parameters.AddWithValue("@nombre", searchQuery);
            return comando;
        }

        public void Buscar(SqlCommand comando)
        {
            using (SqlDataReader lector = comando.ExecuteReader())
            {
                while (lector.Read())
                {
                    Usuarios Usuario = new Usuarios();
                    Usuario.IDUsuario = lector.GetInt32(7);
                    Usuario.ActivoUsuario = lector.GetBoolean(6);
                    Usuario.LoginUsuario = lector.GetString(0);
                    Usuario.NombreUsuario = lector.GetString(1);
                    Usuario.ApellidoUsuario = lector.GetString(2);
                    Usuario.RolUsuario = lector.GetString(3);
                    Usuario.CodEmpleado = lector.GetString(4);
                    Usuario.ClaveUsuario = lector.GetString(8);
                    listaUsuarios.Add(Usuario);
                }
            };
        }

    }
}
