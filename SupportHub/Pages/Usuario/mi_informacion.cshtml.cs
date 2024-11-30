using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportHub.Helpers;
using SupportHub.Modelos;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.CompilerServices;

namespace SupportHub.Pages.Usuario
{
    [Authorize]
    public class mi_informacionModel : PageModel
    {
        [TempData]
        public bool exito { get; set; } = false;
        [TempData]
        public bool exito2 { get; set; } = false;
        public readonly IConfiguration configuracion;
        private readonly Conexiones conexion;
        public List<Usuarios> Usuario = new List<Usuarios>();
		public List<ParametrosCorreo> enviosCorreo = new List<ParametrosCorreo>();
		public String mensajeError = "";

        public mi_informacionModel(IConfiguration configuracion)
        {
            this.configuracion = configuracion;
            conexion = new(this.configuracion);
        }
        public void OnGet()
        {
            try
            {
                string usuario = HttpContext.Session.GetString("usuario");
                string cadena = conexion.ObtenerCadenaDisponible();

                if (string.IsNullOrEmpty(cadena))
                {
                    ViewData["ErrorSQL"] = "El sistema no tiene conexión con el servidor. Favor notifique el impase al administrador.";
                }

                string consulta = "select u.idUsuario, u.loginUsuario, e.nombreEmpleado, e.apellidoEmpleado, r.nombreRol " +
                    "from Empleados e join Usuarios u on e.idEmpleado = u.idEmpleado" +
                    " join UsuariosXRoles ur on u.idUsuario = ur.idUsuario join Roles r" +
                    " on ur.idRol = r.idRol  where u.loginUsuario = @usuario";

                if (!string.IsNullOrEmpty(usuario))
                {
                    using (SqlConnection conexion = new SqlConnection(cadena))
                    {
                        conexion.Open();

                        SqlCommand comando = new SqlCommand(consulta, conexion);
                        comando.Parameters.AddWithValue("@usuario", usuario);

                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            if (lector.Read())
                            {
                                Usuarios newUsuario = new Usuarios();

                                newUsuario.IDUsuario = lector.GetInt32(0);
                                newUsuario.LoginUsuario = lector.GetString(1);
                                newUsuario.NombreUsuario = lector.GetString(2);
                                newUsuario.ApellidoUsuario = lector.GetString(3);
                                newUsuario.RolUsuario = lector.GetString(4);
                                Usuario.Add(newUsuario);
                            }
                        }
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo leer usuario el usuario logueado al sistema.";
                }

                if (User.IsInRole("Administrador"))
                {
                    using (SqlConnection conexion = new SqlConnection(cadena))
                    {
                        conexion.Open();

                        SqlCommand comando = new SqlCommand("select * from DatosEnvioEmail", conexion);
                        //comando.Parameters.AddWithValue("@correo", EnvioCorreo);
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                ParametrosCorreo envio = new ParametrosCorreo
                                {
                                    Correo = lector.GetString(0),
                                    Asunto = lector.GetString(1),
                                    Cuerpo = lector.GetString(2),
                                    Host = lector.GetString(3),
                                    Puerto = lector.GetInt32(4),
                                    Contraseña = lector.GetString(5),
                                    SslActivo = lector.GetBoolean(6)
                                };
                                enviosCorreo.Add(envio);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al obtener datos de usuario: {ex.Message}";
            }
        }

        public IActionResult OnPost()
        {
            string modificarCorreo = Request.Form["esModificacionDeCorreo"];
            string cadena = conexion.ObtenerCadenaDisponible();
            try
            {
                if (modificarCorreo == null)
                {
                    Usuarios setUsuario = new Usuarios();
                    #region Variables
                    string contraseñaActual = Request.Form["contraA"];
                    string nuevaContraseña = Request.Form["nContra"];
                    string ConfirmarNuevaContra = Request.Form["CnContra"];
                    setUsuario.IDUsuario = int.Parse(Request.Form["id"]);
                    setUsuario.NombreUsuario = Request.Form["nombre"];
                    setUsuario.ApellidoUsuario = Request.Form["apellido"];
                    setUsuario.LoginUsuario = Request.Form["usuario"];
                    #endregion

                    using (SqlConnection conexion = new SqlConnection(cadena))
                    {
                        conexion.Open();
                        if (ConfirmarNuevaContra == "")
                        {
                            using (SqlCommand comando = new SqlCommand("UPDATE Usuarios set loginUsuario = @login WHERE idUsuario = @id", conexion))
                            {
                                comando.Parameters.AddWithValue("@login", setUsuario.LoginUsuario);
                                comando.Parameters.AddWithValue("@id", setUsuario.IDUsuario);
                                comando.ExecuteNonQuery();
                                exito = true;
                            }
                        }
                        else
                        {
                            using (SqlCommand comando = new SqlCommand("sp_modificar_contraseña", conexion))
                            {
                                comando.CommandType = CommandType.StoredProcedure;
                                comando.Parameters.AddWithValue("@idUsuario", setUsuario.IDUsuario);
                                comando.Parameters.AddWithValue("@claveUsuario", ConfirmarNuevaContra);
                                comando.Parameters.AddWithValue("@loginUsuario", setUsuario.LoginUsuario);

                                comando.ExecuteNonQuery();
                                exito = true;
                            }
                        }
                    }
                }
                else if (modificarCorreo == "true")
                {
                    if (User.IsInRole("Administrador"))
                    {
                        using (SqlConnection con = new(cadena))
                        {
                            ParametrosCorreo p = new();
                            p.Correo = Request.Form["correo"];
                            p.Asunto = Request.Form["asunto"];
                            p.Cuerpo = Request.Form["cuerpo"];
                            p.Host = Request.Form["host"];
                            p.Puerto = Convert.ToInt32(Request.Form["puerto"]);
                            p.SslActivo = Convert.ToBoolean(Request.Form["ssl"]);
                            p.Contraseña = Request.Form["contra"];

                            con.Open();
                            using (SqlCommand cmd = new("sp_ModificarEmail", con))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@correo", p.Correo);
                                cmd.Parameters.AddWithValue("@asunto", p.Asunto);
                                cmd.Parameters.AddWithValue("@cuerpo", p.Cuerpo);
                                cmd.Parameters.AddWithValue("@host", p.Host);
                                cmd.Parameters.AddWithValue("@puerto", p.Puerto);
                                cmd.Parameters.AddWithValue("@ssl", p.SslActivo);
                                cmd.Parameters.AddWithValue("@contra", p.Contraseña);

                                cmd.ExecuteNonQuery();
                                exito2 = true;
                            }
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "No tiene el rol requerido para realizar esta acción.";
                        return RedirectToPage("/Welcome/Logout");
                    }
                }
                if (exito)
                {
                    return RedirectToPage("/Welcome/Logout");
                }
                else if (exito2)
                {
                    return RedirectToPage("/Usuario/mi_informacion");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ocurrió el siguiente error al momento de procesar la transacción: {ex.Message}.";
                return RedirectToPage("/Usuario/mi_informacion");
            }

            return RedirectToPage("/Usuario/mi_informacion");
        }
    }
}
