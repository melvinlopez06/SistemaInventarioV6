using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SistemaInventarioV6.AccesoDatos.Data;
using SistemaInventarioV6.AccesoDatos.Repositorio;
using SistemaInventarioV6.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventarioV6.Utilidades;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IdentityUser , IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddErrorDescriber<ErrorDescriber>()  //se agrega personalización de mensajes cuando no cumpla requisitos de password
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

//cambiando la politica de passwords
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;  //contraseña no requiere números
    options.Password.RequireLowercase = true; //contraseña requiere letras minusculas
    options.Password.RequireUppercase = false; //contraseña no requiere letras mayúsuclas
    options.Password.RequireNonAlphanumeric = false;  //contraseña no requiere caracteres especiales
    options.Password.RequiredLength = 6;  //contraseña requiere 6 caracteres como mínimo
    options.Password.RequiredUniqueChars = 1;   //contraseña solo puede repetir 1 vez un mismo caracter
});
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

//agregar como servicio a la unidad de trabajo para que cargue sus repositorios
builder.Services.AddScoped<IUnidadTrabajo, UnidadTrabajo>();

//agregar el servicio de las razor pages
builder.Services.AddRazorPages();

//agregar el servicio de email sender
builder.Services.AddSingleton<IEmailSender , EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   //habilitar autenticacion de usuarios
app.UseAuthorization();     //habilitar autorizacion de partes del site a usuarios autenticados

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Inventario}/{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
