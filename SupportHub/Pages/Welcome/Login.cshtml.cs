using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using SupportHub.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using SupportHub.Modelos;
namespace SupportHub.Pages.Welcome
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration configuracion;
        private readonly Conexiones conexion;
        private readonly ILogger<LoginModel> _logger;
        public LoginModel(ILogger<LoginModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.configuracion = configuration;
            conexion = new(this.configuracion);
        }

        [BindProperty]
        public string Username { get; set; }
        [BindProperty]
        public string Password { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            int idUsuarioLogueado;
            string cadena = conexion.ObtenerCadenaDisponible();
            if (string.IsNullOrEmpty(cadena))
            {
                ViewData["ErrorMessage"] = "El sistema no tiene conexión con el servidor. Favor notifique el impase al administrador.";
                return Page();
            }

            try
            {
                HttpContext.Session.SetString("usuario", Username);
                HttpContext.Session.SetString("contra", Password);
                using (SqlConnection conn = new SqlConnection(cadena))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("sp_autenticar_usuario", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LoginName", Username);
                    cmd.Parameters.AddWithValue("@Password", Password);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())  // Si el procedimiento devuelve una fila
                        {
                            // Obtener el nombre y apellido del usuario
                            string nombreCompleto = reader["nombreEmpleado"].ToString() + " " + reader["apellidoEmpleado"].ToString();
                            string rol = reader["nombreRol"].ToString();
                            int idEmpleado = Convert.ToInt32(reader["idEmpleado"]);
                            bool activo =(bool)reader["activoUsuario"];
                            idUsuarioLogueado = Convert.ToInt32(reader["idUsuario"]);
                            if (activo) {
                                // Crear los claims del usuario autenticado
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, nombreCompleto), // Usar el nombre completo
                                    new Claim(ClaimTypes.Role, rol)
                                };

                                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                                // Autenticar al usuario creando la cookie
                                await
                                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                                //guardar el id en la sesión
                                HttpContext.Session.SetInt32("id",idUsuarioLogueado);
                                HttpContext.Session.SetInt32("idEmpleado", idEmpleado);
                                // Redirigir al usuario autenticado
                                return Redirect("/Index");
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                                ViewData["ErrorMessage"] = "Usuario inactivo."; // Mensaje de error
                                return Page();
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                            ViewData["ErrorMessage"] = "Usuario o contraseña incorrecta."; // Mensaje de error
                            return Page();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Error al autenticar usuario: {ex.Message}";
                return Page();
            }
        }
    }
}
