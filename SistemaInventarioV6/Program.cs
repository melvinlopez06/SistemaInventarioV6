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
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Inventario}/{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
