using Microsoft.EntityFrameworkCore;
using SistemaReportesWeb.Data;
using SistemaReportesWeb.Services;
using SistemaReportesWeb.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ✅ SERVICIO EXCEL CORREGIDO - Usando INTERFAZ
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

// ✅ AGREGAR SERVICIO DE SESIONES
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Registrar el DbContext con MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

var app = builder.Build();

app.UseCors("AllowAll");

// ✅ SOLUCIÓN DEFINITIVA - ELIMINAR CSP PARA DESARROLLO
if (app.Environment.IsDevelopment())
{
    // En desarrollo: sin CSP restrictivo
    app.Use(async (context, next) =>
    {
        // NO agregar headers CSP en desarrollo
        await next();
    });
}
else
{
    // En producción: CSP seguro
  //  app.Use(async (context, next) =>
 //   {
   //     context.Response.Headers.Append("Content-Security-Policy",
  //          "default-src 'self'; " +
  //          "script-src 'self' 'unsafe-inline'; " +
  //          "style-src 'self' 'unsafe-inline';");
  //      await next();
 //   });
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// ✅ AGREGAR SESIONES Y AUTENTICACIÓN
app.UseSession();
app.UseMiddleware<AuthMiddleware>();

app.UseAuthorization();

// ✅ CAMBIAR RUTA POR DEFECTO A LOGIN
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();