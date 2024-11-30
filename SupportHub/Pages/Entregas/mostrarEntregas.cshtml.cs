using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SupportHub.Helpers;
using SupportHub.Modelos;
using System.Data.SqlClient;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System.Reflection.Metadata.Ecma335;
using System.Globalization;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace SupportHub.Pages.Entregas
{
    [Authorize(Roles = "Administrador,Común,Técnico")]
    public class mostrarEntregasModel : PageModel
    {
        Empleados EmpRecibe = new();
        Empleados EmpEntrega = new();
        private readonly IConfiguration configuracion;
        private readonly Conexiones conexion;
        public List<Entrega> listaEntregas = new();
        public List<Empleados> ListaEmp = new();
        public List<Entrega> EntregasLista = new List<Entrega>();
        public Entrega Emp = new();
        public string mensajeError { get; set; } = "";
        [TempData]
        public bool exito { get; set; } = false;
        [TempData]
        public bool eliminada { get; set; } = false;
        [TempData]
        public bool devolucion { get; set; } = false;
        [TempData]
        public bool devolucionEliminada { get; set; } = false;

        public mostrarEntregasModel(IConfiguration configuracion)
        {
            this.configuracion = configuracion;
            conexion = new(this.configuracion);
        }

        public void LlenaDatos(int id, ref Entrega DatosEntrega)
        {
            string cadena = conexion.ObtenerCadenaDisponible();
            if (string.IsNullOrEmpty(cadena))
            {
                ViewData["errorConexion"] = "El sistema no tiene conexión con el servidor. Favor notifique el impase al administrador.";
            }

            try
            {
                using (SqlConnection conexion = new(cadena))
                {
                    conexion.Open();
                    using (SqlCommand comando = new("dbo.sp_obtener_entregas_id2 @Id", conexion))
                    {
                        comando.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                DatosEntrega.idEntrega = lector.GetInt32(0);
                                DatosEntrega.codEntrega = lector.GetString(1);
                                DatosEntrega.nombreTipoEntrega = lector.GetString(2);
                                DatosEntrega.cantidadEntrega = lector.GetInt32(3);
                                DatosEntrega.idEmpleadoEntrega = lector.GetInt32(4);
                                DatosEntrega.nombreEmpleadoEntrego = lector.GetString(5);
                                DatosEntrega.idEmpleadoRecibe = lector.GetInt32(6);
                                DatosEntrega.nombreEmpleadoRecibio = lector.GetString(7);
                                DatosEntrega.fechaEntrega = lector.GetDateTime(8).ToString("dd-MM-yyyy");
                                DatosEntrega.fechaDevolucion = lector.IsDBNull(9) ? string.Empty : lector.GetDateTime(9).ToString("dd-MM-yyyy");
                                DatosEntrega.observacionEntrega = lector.GetString(10);
                                DatosEntrega.idEquipo = lector.GetInt32(11);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["errorSQL"] = $"Ocurrió el siguiente error al momento de descargar el reporte: {ex.Message}.";
            }
        }
        public void LlenaDatosEquipo(int id, ref Equipos DatosEquipo)
        {
            string cadena = conexion.ObtenerCadenaDisponible();
            if (string.IsNullOrEmpty(cadena))
            {
                ViewData["errorConexion"] = "El sistema no tiene conexión con el servidor. Favor notifique el impase al administrador.";
            }

            try
            {
                using (SqlConnection conexion = new(cadena))
                {
                    conexion.Open();
                    using (SqlCommand comando = new("dbo.sp_obtener_Equipos2 @Id", conexion))
                    {
                        comando.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                DatosEquipo.codEquipo = lector.GetString(0);
                                DatosEquipo.marcaEquipo = lector.GetString(1);
                                DatosEquipo.modeloEquipo = lector.GetString(2);
                                DatosEquipo.cantidadAdquirida = lector.GetInt32(3);
                                DatosEquipo.precioEquipo = Math.Round(lector.GetDecimal(4),2);
                                DatosEquipo.tipoEquipo = lector.GetString(5);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["errorSQL"] = $"Ocurrió el siguiente error al momento de descargar el reporte: {ex.Message}.";
            }
        }
        public void OnGet()
        {
            string cadena = conexion.ObtenerCadenaDisponible();
            if (string.IsNullOrEmpty(cadena))
            {
                ViewData["errorConexion"] = "El sistema no tiene conexión con el servidor. Favor notifique el impase al administrador.";
            }

            try
            {
                using (SqlConnection conexion = new(cadena))
                {
                    conexion.Open();
                    using (SqlCommand comando = new("[dbo].[sp_obtener_entregas]", conexion))
                    {
                        comando.CommandType = System.Data.CommandType.StoredProcedure;
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                Entrega Emp = new();
                                Emp.idEntrega = lector.GetInt32(0);
                                Emp.codEntrega = lector.GetString(1);
                                Emp.nombreTipoEntrega = lector.GetString(2);
                                Emp.equipo = lector.GetString(3);
                                Emp.cantidadEntrega = lector.GetInt32(4);
                                Emp.nombreEmpleadoEntrego = lector.GetString(5);
                                Emp.nombreEmpleadoRecibio = lector.GetString(6);
                                Emp.fechaEntrega = lector.IsDBNull(7) ? string.Empty : lector.GetDateTime(7).ToString("dd-MM-yyyy");
                                Emp.fechaDevolucion = lector.IsDBNull(8) ? string.Empty : lector.GetDateTime(8).ToString("dd-MM-yyyy");
                                Emp.observacionEntrega = lector.IsDBNull(9) ? string.Empty : lector.GetString(9);
                                Emp.idTipoEntrega = lector.GetInt32(10);
                                Emp.idEquipo = lector.GetInt32(11);
                                Emp.idEmpleadoEntrega = lector.GetInt32(12);
                                Emp.idEmpleadoRecibe = lector.GetInt32(13);
                                listaEntregas.Add(Emp);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["errorSQL"] = "Error al obtener entregas: " + ex.Message;
            }
        }

        public string GetTiposDeEntregas()
        {
            try
            {
                string cadena = conexion.ObtenerCadenaDisponible();
                if (string.IsNullOrEmpty(cadena))
                {
                    ViewData["errorConexion"] = "El sistema no tiene conexión con el servidor. Favor notifique el impase al administrador.";
                    return "";
                }

                List<SelectListItem> tiposDeEntregas = [];
                using (SqlConnection conection = new(cadena))
                {
                    conection.Open();
                    using (SqlCommand comando = new("select * from dbo.TiposEntregas", conection))
                    {
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                tiposDeEntregas.Add(new SelectListItem
                                {
                                    Value = lector.GetInt32(0).ToString(),
                                    Text = lector.GetString(1)
                                });
                            }
                        }
                    }
                }

                var sb = new StringBuilder();
                if (tiposDeEntregas != null)
                {
                    foreach (var tipoEntrega in tiposDeEntregas)
                    {
                        sb.Append($"<option value='{tipoEntrega.Value}'>{tipoEntrega.Text}</option>");
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                ViewData["errorSQL"] = "Error al obtener tipos de entregas: " + ex.Message;
                return string.Empty;
            }
        }

        public string GetEmpleados()
        {
            try
            {
                string cadena = conexion.ObtenerCadenaDisponible();
                if (string.IsNullOrEmpty(cadena))
                {
                    ViewData["errorConexion"] = "El sistema no tiene conexión con el servidor. Favor notifique el impase al administrador.";
                    return "";
                }

                List<SelectListItem> empleados = [];
                using (SqlConnection conection = new(cadena))
                {
                    conection.Open();
                    using (SqlCommand comando = new("select idEmpleado, codEmpleado + ' - ' + nombreEmpleado + ' ' + apellidoEmpleado from dbo.Empleados where empleadoActivo = 1", conection))
                    {
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                empleados.Add(new SelectListItem
                                {
                                    Value = lector.GetInt32(0).ToString(),
                                    Text = lector.GetString(1)
                                });
                            }
                        }
                    }
                }

                var sb = new StringBuilder();
                if (empleados != null)
                {
                    foreach (var empleado in empleados)
                    {
                        sb.Append($"<option value='{empleado.Value}'>{empleado.Text}</option>");
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                ViewData["errorSQL"] = "Error error al obtener empleados: " + ex.Message;
                return string.Empty;
            }
        }

        public string GetEquipos()
        {
            try
            {
                string cadena = conexion.ObtenerCadenaDisponible();
                if (string.IsNullOrEmpty(cadena))
                {
                    ViewData["errorConexion"] = "El sistema no tiene conexión con el servidor. Favor notifique el impase al administrador.";
                    return "";
                }

                var sb = new StringBuilder();
                using (SqlConnection conection = new(cadena))
                {
                    conection.Open();
                    using (SqlCommand comando = new("select E.idEquipo, E.codEquipo + ' - ' + E.tipoEquipo + ' ' + E.marcaEquipo + ' ' + E.modeloEquipo, ED.cantidadDisponible from dbo.Equipos E inner join dbo.vwCantidadEquiposDisponibles ED on E.idEquipo = ED.idEquipo", conection))
                    {
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                sb.Append($"<option data-disponible='{lector.GetInt32(2)}' value='{lector.GetInt32(0)}'>{lector.GetString(1)} ({lector.GetInt32(2)} disponibles)</option>");
                            }
                        }
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                ViewData["errorSQL"] = "Error al obtener equipos: " + ex.Message;
                return string.Empty;
            }
        }

        public IActionResult OnPost()
        {
            if (User.IsInRole("Administrador") || User.IsInRole("Técnico"))
            {
                string cadena = conexion.ObtenerCadenaDisponible();
                if (string.IsNullOrEmpty(cadena))
                {
                    ViewData["errorConexion"] = "El sistema no tiene conexión con el servidor. Favor notifique el impase al administrador.";
                    return Page();
                }

                Emp.codEntrega = Request.Form["codEntrega"].ToString();

                try
                {
                    int registrosAlterados = 0;
                    int registrosEliminados = 0;
                    int devolucionesAgregadas = 0;
                    int devolucionesEliminadas = 0;
                    if (Request.Form["esEliminacion"] == "true")
                    {   
                        if (User.IsInRole("Administrador"))
                        {
                            using (SqlConnection conexion = new(cadena))
                            {
                                conexion.Open();
                                SqlCommand comando = new("dbo.sp_eliminar_entrega @codEntrega", conexion);

                                comando.Parameters.AddWithValue("@codEntrega", Emp.codEntrega);

                                registrosEliminados = Convert.ToInt32(comando.ExecuteScalar().ToString());
                            }
                        }
                    }
                    else if (Request.Form["esDevolucion"] == "true")
                    {
                        DateTime fechaDevolucion = DateTime.ParseExact(Request.Form["fechaDevolucion"], "yyyy-MM-dd", CultureInfo.InvariantCulture);

                        using (SqlConnection conexion = new(cadena))
                        {
                            conexion.Open();
                            SqlCommand comando = new("update dbo.Entregas set fechaDevolucion = @fechaDevolucion where codEntrega = @codEntrega", conexion);

                            comando.Parameters.AddWithValue("@codEntrega", Emp.codEntrega);
                            comando.Parameters.AddWithValue("@fechaDevolucion", fechaDevolucion);

                            devolucionesAgregadas = comando.ExecuteNonQuery();
                        }
                    }
                    else if (Request.Form["esEliminacionDevolucion"] == "true")
                    {
                        using (SqlConnection conexion = new(cadena))
                        {
                            conexion.Open();
                            SqlCommand comando = new("update dbo.Entregas set fechaDevolucion = null where codEntrega = @codEntrega", conexion);

                            comando.Parameters.AddWithValue("@codEntrega", Emp.codEntrega);

                            devolucionesEliminadas = comando.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        Emp.idTipoEntrega = Convert.ToInt32(Request.Form["idTipoEntrega"]);
                        //Emp.fechaEntrega = Convert.ToDateTime(Request.Form["fechaEntrega"]).ToString("yyyy-MM-dd");
                        DateTime fechaEntrega = DateTime.ParseExact(Request.Form["fechaEntrega"], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        Emp.idEmpleadoRecibe = Convert.ToInt32(Request.Form["idEmpleadoRecibe"]);
                        Emp.idEquipo = Convert.ToInt32(Request.Form["idEquipo"]);
                        Emp.cantidadEntrega = Convert.ToInt32(Request.Form["cantidadEntrega"]);
                        Emp.observacionEntrega = Request.Form["observacionEntrega"].ToString();
                        if (Request.Form["esModificacion"] == "true")
                        {
                            using (SqlConnection conexion = new(cadena))
                            {
                                conexion.Open();
                                SqlCommand comando = new("dbo.sp_modificar_entrega @codEntrega, @idTipoEntrega, @cantidadEntrega, @fechaEntrega, @observacionEntrega, @idEmpleadoRecibe, @idEquipo", conexion);

                                comando.Parameters.AddWithValue("@codEntrega", Emp.codEntrega);
                                comando.Parameters.AddWithValue("@idTipoEntrega", Emp.idTipoEntrega);
                                comando.Parameters.AddWithValue("@fechaEntrega", fechaEntrega);
                                comando.Parameters.AddWithValue("@idEmpleadoRecibe", Emp.idEmpleadoRecibe);
                                comando.Parameters.AddWithValue("@idEquipo", Emp.idEquipo);
                                comando.Parameters.AddWithValue("@cantidadEntrega", Emp.cantidadEntrega);
                                comando.Parameters.AddWithValue("@observacionEntrega", Emp.observacionEntrega);

                                registrosAlterados = Convert.ToInt32(comando.ExecuteScalar().ToString());
                            }
                        }
                        else
                        {
                            using (SqlConnection conexion = new(cadena))
                            {
                                conexion.Open();
                                SqlCommand comando = new("dbo.sp_crear_entrega @idTipoEntrega, @fechaEntrega, @idEmpleadoEntrega, @idEmpleadoRecibe, @idEquipo, @cantidadEntrega, @observacionEntrega", conexion);

                                comando.Parameters.AddWithValue("@idTipoEntrega", Emp.idTipoEntrega);
                                comando.Parameters.AddWithValue("@cantidadEntrega", Emp.cantidadEntrega);
                                comando.Parameters.AddWithValue("@fechaEntrega", fechaEntrega);
                                comando.Parameters.AddWithValue("@observacionEntrega", Emp.observacionEntrega);
                                comando.Parameters.AddWithValue("@idEmpleadoEntrega", HttpContext.Session.GetInt32("idEmpleado"));
                                comando.Parameters.AddWithValue("@idEmpleadoRecibe", Emp.idEmpleadoRecibe);
                                comando.Parameters.AddWithValue("@idEquipo", Emp.idEquipo);

                                registrosAlterados = Convert.ToInt32(comando.ExecuteScalar().ToString());
                            }
                        }
                    }
                    exito = registrosAlterados == 1;
                    eliminada = registrosEliminados == 1;
                    devolucion = devolucionesAgregadas == 1;
                    devolucionEliminada = devolucionesEliminadas == 1;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Ocurrió el siguiente error al momento de procesar la transacción: {ex.Message}.";
                    return RedirectToPage("/Entregas/mostrarEntregas");
                }
            }
            return RedirectToPage("/Entregas/mostrarEntregas");
        }

        // Método independiente para llenar los datos de la entrega
        private Entrega LlenaDatosEntrega(int id)
        {
            Entrega Datos = new Entrega();
            LlenaDatos(id, ref Datos);
            return Datos;
        }

        private Equipos LlenaDatosEquipo(int id)
        {
            Equipos DatosEquipo = new Equipos();
            LlenaDatosEquipo(id, ref DatosEquipo);
            return DatosEquipo;
        }

        // Método para descargar el PDF
        public IActionResult OnGetDownloadFile(int id)
        {
            Entrega Datos = LlenaDatosEntrega(id);
            Equipos Equipo = LlenaDatosEquipo(Datos.idEquipo);
            byte[] pdfBytes = GenerarPDF(Datos, Equipo);

            var fileName = $"Entrega_{id}.pdf";
            var mimeType = "application/pdf";
            return File(pdfBytes, mimeType, fileName);
        }

        // Método para enviar el PDF por correo
        public IActionResult OnGetEnviarPorCorreo(int id)
        {
            string correoDestino = CorreoEmpleado(id)  ;

            Entrega Datos = LlenaDatosEntrega(id);
            Equipos Equipo = LlenaDatosEquipo(Datos.idEquipo);
            byte[] pdfBytes = GenerarPDF(Datos, Equipo);

            EnviarCorreoConAdjunto(pdfBytes, $"Entrega_{id}.pdf", correoDestino);
            TempData["SuccessMessage"] = "¡El correo se envió correctamente!"; 

            return RedirectToPage("/Entregas/mostrarEntregas");
        }

        private byte[] GenerarPDF(Entrega Datos,Equipos Equipo)
        {
            DatosEmpleadoEntrega(Datos.idEmpleadoEntrega);
            DatosEmpleadoRecibe(Datos.idEmpleadoRecibe);
            using (MemoryStream stream = new MemoryStream())
            {
                Document pdfDoc = new Document(PageSize.A4);
                PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.AddCreator("SupportHub");
                pdfDoc.Open();

                // Estilos
                Font titleFont = FontFactory.GetFont("Arial", 14, Font.BOLD);
                Font headerFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
                Font bodyFont = FontFactory.GetFont("Arial", 12, Font.NORMAL);
                iTextSharp.text.Font _standardFont;

                // Imagen del logo
                string currentDirectory = Directory.GetCurrentDirectory();
                string imagePath = Path.Combine(currentDirectory, "wwwroot", "imagenes", "logoSupportHub2.jpeg");
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imagePath);
                img.ScaleToFit(150f, 150f);
                img.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
                pdfDoc.Add(img);

                // Tí­tulo del reporte
                Paragraph title = new Paragraph("Reporte de Entrega de Equipos", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                pdfDoc.Add(title);

                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);
          
                // Información de dirección
                _standardFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                Paragraph Direccion = new Paragraph("Torre Futura, 87 Avenida Norte y Calle El Mirador, \nColonia Escalón, Oficina 555 San Salvador, El Salvador.", _standardFont);
                Direccion.Alignment = Element.ALIGN_RIGHT;
                pdfDoc.Add(Direccion);

                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);

                _standardFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                pdfDoc.Add(new Paragraph("Codigo Entrega: " + Datos.codEntrega, _standardFont));
                pdfDoc.Add(Chunk.NEWLINE);

                _standardFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                pdfDoc.Add(new Paragraph("IT Encargado: " + EmpRecibe.nombreEmpleado+ " " +EmpRecibe.apellidoEmpleado +" - "+EmpRecibe.codEmpleado, _standardFont));
                pdfDoc.Add(Chunk.NEWLINE);

                _standardFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                pdfDoc.Add(new Paragraph("Usuario Entrega: " + EmpEntrega.nombreEmpleado+" "+ EmpEntrega.apellidoEmpleado +" - "+EmpEntrega.codEmpleado, _standardFont));
                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);

                PdfPTable tblPrueba = new PdfPTable(5);

                // Encabezados
                PdfPCell clCodigoEquipo = new PdfPCell(new Phrase("Código \nEquipo", _standardFont));
                clCodigoEquipo.BorderWidth = 0;
                clCodigoEquipo.BorderWidthBottom = 0.5f;
                clCodigoEquipo.BackgroundColor = BaseColor.LIGHT_GRAY;
                clCodigoEquipo.HorizontalAlignment = Element.ALIGN_CENTER;

                PdfPCell clModeloEquipo = new PdfPCell(new Phrase("Marca - Modelo\nEquipo", _standardFont));
                clModeloEquipo.BorderWidth = 0;
                clModeloEquipo.BorderWidthBottom = 0.5f;
                clModeloEquipo.BackgroundColor = BaseColor.LIGHT_GRAY;
                clModeloEquipo.HorizontalAlignment = Element.ALIGN_CENTER;

                PdfPCell clCodEquipo = new PdfPCell(new Phrase("Cantidad\nEquipo", _standardFont));
                clCodEquipo.BorderWidth = 0;
                clCodEquipo.BorderWidthBottom = 0.5f;
                clCodEquipo.BackgroundColor = BaseColor.LIGHT_GRAY;
                clCodEquipo.HorizontalAlignment = Element.ALIGN_CENTER;

                PdfPCell clCanEquipo = new PdfPCell(new Phrase("Tipo\nEquipo", _standardFont));
                clCanEquipo.BorderWidth = 0;
                clCanEquipo.BorderWidthBottom = 0.5f;
                clCanEquipo.BackgroundColor = BaseColor.LIGHT_GRAY;
                clCanEquipo.HorizontalAlignment = Element.ALIGN_CENTER;

                PdfPCell clPrecioEquipo = new PdfPCell(new Phrase("Tipo\nEntrega", _standardFont));
                clPrecioEquipo.BorderWidth = 0;
                clPrecioEquipo.BorderWidthBottom = 0.5f;
                clPrecioEquipo.BackgroundColor = BaseColor.LIGHT_GRAY;
                clPrecioEquipo.HorizontalAlignment = Element.ALIGN_CENTER;

                // Agregamos las celdas de los encabezados a la tabla
                tblPrueba.AddCell(clCodigoEquipo);
                tblPrueba.AddCell(clModeloEquipo);
                tblPrueba.AddCell(clCodEquipo);
                tblPrueba.AddCell(clCanEquipo);
                tblPrueba.AddCell(clPrecioEquipo);
              
                // Ahora agregamos los datos
                // Ahora agregamos los datos en la siguiente fila
                PdfPCell clDatoIDEquipo = new PdfPCell(new Phrase(Convert.ToString(Equipo.codEquipo), _standardFont));
                clDatoIDEquipo.BorderWidth = 0;
                clDatoIDEquipo.HorizontalAlignment = Element.ALIGN_CENTER;

                PdfPCell clDatoMarca = new PdfPCell(new Phrase(Convert.ToString(Equipo.marcaEquipo), _standardFont));
                clDatoMarca.BorderWidth = 0;
                clDatoMarca.HorizontalAlignment = Element.ALIGN_CENTER;

                PdfPCell clDatoCantidad = new PdfPCell(new Phrase(Convert.ToString(Datos.cantidadEntrega), _standardFont));
                clDatoCantidad.BorderWidth = 0;
                clDatoCantidad.HorizontalAlignment = Element.ALIGN_CENTER;

                PdfPCell clDatoModelo = new PdfPCell(new Phrase(Convert.ToString(Equipo.modeloEquipo), _standardFont));
                clDatoModelo.BorderWidth = 0;
                clDatoModelo.HorizontalAlignment = Element.ALIGN_CENTER;

                PdfPCell clDatoTipoEntrega = new PdfPCell(new Phrase(Convert.ToString(Datos.nombreTipoEntrega), _standardFont));
                clDatoTipoEntrega.BorderWidth = 0;
                clDatoTipoEntrega.HorizontalAlignment = Element.ALIGN_CENTER;

                // Agregar los datos a la tabla en la fila debajo de los encabezados
                tblPrueba.AddCell(clDatoIDEquipo);
                tblPrueba.AddCell(clDatoMarca);
                tblPrueba.AddCell(clDatoCantidad);
                tblPrueba.AddCell(clDatoModelo);
                tblPrueba.AddCell(clDatoTipoEntrega);
               
                // Agregamos la tabla al documento PDF
                pdfDoc.Add(tblPrueba);
                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);
          
                _standardFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                pdfDoc.Add(new Paragraph("OBSERVACIONES: ", _standardFont));
                _standardFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                pdfDoc.Add(new Paragraph(Datos.observacionEntrega, _standardFont));
                pdfDoc.Add(new Paragraph("                                                                                                                                                                              ", _standardFont));
                pdfDoc.Add(new Paragraph("                                                                                                                                                                              ", _standardFont));
                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);
                _standardFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                pdfDoc.Add(new Paragraph("F.IT: _____________________________                                 F.Empleado: ___________________________", _standardFont));

                // Cerrar documento PDF
                pdfDoc.Add(new Paragraph("\n"));
                pdfDoc.Close();

                return stream.ToArray();
            }
        }

        private void EnviarCorreoConAdjunto(byte[] archivoAdjunto, string nombreArchivo, string CorreoDestino)
        {
            try
            {
                string cadena = conexion.ObtenerCadenaDisponible();
                ParametrosCorreo PrmCorreo = new ParametrosCorreo();

                using (SqlConnection conexion = new(cadena))
                {
                    conexion.Open();
                    using (SqlCommand comando = new("Select * from DatosEnvioEmail", conexion))
                    {
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                PrmCorreo.Correo = lector.GetString(0);
                                PrmCorreo.Asunto = lector.GetString(1);
                                PrmCorreo.Cuerpo = lector.GetString(2);
                                PrmCorreo.Host = lector.GetString(3);
                                PrmCorreo.Puerto = lector.GetInt32(4);
                                PrmCorreo.Contraseña = lector.GetString(5);
                                PrmCorreo.SslActivo = lector.GetBoolean(6);
                            }
                        }
                    }
                }

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(CorreoDestino);
                mail.To.Add(CorreoDestino);
                mail.Subject = PrmCorreo.Asunto;
                mail.Body = "<h1>Este es tu informe PDF</h1><p>Adjunto encontrarás el documento solicitado.</p>";
                mail.IsBodyHtml = true;

                // Adjuntar el archivo PDF
                MemoryStream ms = new MemoryStream(archivoAdjunto);
                Attachment attachment = new Attachment(ms, nombreArchivo, "application/pdf");
                mail.Attachments.Add(attachment);

                // Configurar el cliente SMTP
                SmtpClient smtpClient = new SmtpClient(PrmCorreo.Host);
                smtpClient.Port = PrmCorreo.Puerto; 
                smtpClient.Credentials = new NetworkCredential(PrmCorreo.Correo, PrmCorreo.Contraseña);
                smtpClient.EnableSsl = PrmCorreo.SslActivo;

                // Enviar el correo
                smtpClient.Send(mail);
            }
            catch (Exception ex)
            {
                // Manejar el error de enví­o de correo
                Console.WriteLine("Error al enviar el correo: " + ex.Message);
            }
        }

        public string CorreoEmpleado(int id)
        {
            string correo = "";
            string cadena = conexion.ObtenerCadenaDisponible();
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "select e.emailEmpleado from entregas em inner join empleados e on em.idEmpleadoRecibe = e.idEmpleado where em.idEntrega = @id";
                SqlCommand comando = new SqlCommand(query, con);
                comando.Parameters.AddWithValue("@id", id);

                con.Open();

                correo = (string)comando.ExecuteScalar();

                con.Close();
            }
            
            return correo;
        }

        public void DatosEmpleadoRecibe(int id)
        {
            string cadena = conexion.ObtenerCadenaDisponible();
            if (string.IsNullOrEmpty(cadena))
            {
                ViewData["errorConexion"] = "El sistema no tiene conexión con el servidor. Favor notifique el impase al administrador.";
            }

            try
            {
                using (SqlConnection conexion = new(cadena))
                {
                    conexion.Open();
                    using (SqlCommand comando = new("sp_obtener_Empleados " + id, conexion))
                    {
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                EmpRecibe.idEmpleado = lector.GetInt32(0);
                                EmpRecibe.codEmpleado = lector.GetString(1);
                                EmpRecibe.nombreEmpleado = lector.GetString(2);
                                EmpRecibe.apellidoEmpleado = lector.GetString(3);
                                EmpRecibe.telefonoEmpleado = lector.GetString(4);
                                EmpRecibe.emailEmpleado = lector.GetString(5);
                                EmpRecibe.IdArea = lector.GetInt32(6);
                                EmpRecibe.codArea = lector.GetString(7);
                                EmpRecibe.nombreArea = lector.GetString(8);
                                EmpRecibe.IdCargo = lector.GetInt32(9);
                                EmpRecibe.codCargo = lector.GetString(10);
                                EmpRecibe.nombreCargo = lector.GetString(11);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["errorSQL"] = "Error: " + ex.Message;
            }
        }

        public void DatosEmpleadoEntrega(int id)
        {
            string cadena = conexion.ObtenerCadenaDisponible();
            if (string.IsNullOrEmpty(cadena))
            {
                ViewData["errorConexion"] = "El sistema no tiene conexión con el servidor. Favor notifique el impase al administrador.";
            }

            try
            {
                using (SqlConnection conexion = new(cadena))
                {
                    conexion.Open();
                    using (SqlCommand comando = new("sp_obtener_Empleados "+id, conexion))
                    {
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                EmpEntrega.idEmpleado = lector.GetInt32(0);
                                EmpEntrega.codEmpleado = lector.GetString(1);
                                EmpEntrega.nombreEmpleado = lector.GetString(2);
                                EmpEntrega.apellidoEmpleado = lector.GetString(3);
                                EmpEntrega.telefonoEmpleado = lector.GetString(4);
                                EmpEntrega.emailEmpleado = lector.GetString(5);
                                EmpEntrega.IdArea = lector.GetInt32(6);
                                EmpEntrega.codArea = lector.GetString(7);
                                EmpEntrega.nombreArea = lector.GetString(8);
                                EmpEntrega.IdCargo = lector.GetInt32(9);
                                EmpEntrega.codCargo = lector.GetString(10);
                                EmpEntrega.nombreCargo = lector.GetString(11);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["errorSQL"] = "Error al obtener datos de empleados para la entrega: " + ex.Message;
            }
        }
    }
}