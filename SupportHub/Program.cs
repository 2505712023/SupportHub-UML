using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Configura los servicios
builder.Services.AddRazorPages();

//añade el servicio de sesiones 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5); //la sesión durará 5 minutos
    options.Cookie.HttpOnly = true; //evita que la cookie de sesión sea accesible desde JavaScrip en el cliente
    options.Cookie.IsEssential = true; //marca la sesión como escencial para que no sea bloqueada
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Welcome/Login"; // Ruta para la página de inicio de sesión
        options.LogoutPath = "/Welcome/Logout"; // Ruta para la página de logout
    });

var app = builder.Build();

// Configura el middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
    await next();
});

app.UseRouting();

app.UseAuthentication(); // Asegúrate de que este middleware está aquí
app.UseAuthorization();

app.MapGet("/", (context) =>
{
    context.Response.Redirect("/Welcome/Login");
    return Task.CompletedTask;
});

app.UseSession();

// Map Razor Pages
app.MapRazorPages();

app.Run();
