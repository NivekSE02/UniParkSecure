using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniParkSecure.Data;
using UniParkSecure.Models;
using UniParkSecure.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Cadena de conexión
builder.Configuration["ConnectionStrings:DefaultConnection"] =
    "Server=.\\SQLEXPRESS01;Database=UniParkSecure;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddControllersWithViews(options =>
{
    var policy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
});

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<Usuario, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// MVC y SignalR
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Configuración de cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Home/Login";
    options.LogoutPath = "/Home/Logout";
    options.AccessDeniedPath = "/Home/AccessDenied";
});

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Rutas MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

// RUTA PARA API/CONTROLADORES (asegura rutas tipo /Registros/ConfirmarSector)
app.MapControllers();

// Inicializar datos
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

    // Sectores iniciales
    // Sectores iniciales
    if (!db.Sectores.Any())
    {
        db.Sectores.AddRange(
            new Sector { Nombre = "A", TotalEspacios = 50, Disponibles = 50 },
            new Sector { Nombre = "B", TotalEspacios = 40, Disponibles = 40 },
            new Sector { Nombre = "C", TotalEspacios = 30, Disponibles = 30 },
            new Sector { Nombre = "V", TotalEspacios = 20, Disponibles = 20 }
        );
        db.SaveChanges();
    }


    // Roles
    string[] roles = { "Admin", "UsuarioComun" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Usuario común inicial
    var usuarioEmail = "usuario@unicaes.edu.sv";
    var usuario = await userManager.FindByEmailAsync(usuarioEmail);
    if (usuario == null)
    {
        usuario = new Usuario
        {
            UserName = usuarioEmail,
            Email = usuarioEmail,
            DUI = "11111111-1",
            PlantillaFacial = new byte[0],
            NombreCompleto = "Usuario",
            Apellidos = "Común"
        };
        await userManager.CreateAsync(usuario, "Usuario123!");
        await userManager.AddToRoleAsync(usuario, "UsuarioComun");
    }

    // Admin inicial
    var adminEmail = "admin@unicaes.edu.sv";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new Usuario
        {
            UserName = adminEmail,
            Email = adminEmail,
            DUI = "00000000-0",
            PlantillaFacial = new byte[0],
            NombreCompleto = "Administrador",
            Apellidos = "Principal"
        };
        await userManager.CreateAsync(admin, "Admin123!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}

// SignalR Hub
app.MapHub<ParqueoHub>("/parqueoHub");

app.Run();
