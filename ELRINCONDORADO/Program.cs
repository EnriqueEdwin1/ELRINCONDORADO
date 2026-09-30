using System.Globalization;
using Microsoft.AspNetCore.Localization;
using QuestPDF.Infrastructure;

// Npgsql 6+ exige UTC para "timestamp with time zone"; las columnas usan "timestamp without time zone"
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Secretos locales (user-secrets), donde vive la clave de ImgBB y otras
// credenciales que por seguridad no deben estar en appsettings.json.
builder.Configuration.AddUserSecrets<Program>();

// ===== HttpClient para comunicarse con la API =====
builder.Services.AddHttpClient("ElRinconDoradoAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7166");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Add services to the container.
// El MVC ya no usa DbContext directamente - todos los datos vienen de la API.

// Agregar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

// ===== Registrar servicios API =====
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.AuthApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.ProductosApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.PedidosApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.CategoriasApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.InsumosApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.ProveedoresApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.MesasApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.EmpleadosApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.ComprasApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.CierresCajaApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.MovimientosInventarioApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.RolesApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.PromocionesApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.RecetasApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.VentasApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.CocinaApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.MeseroApiService>();
builder.Services.AddScoped<ELRINCONDORADO.Services.Api.ClientesApiService>();

builder.Services.AddSignalR();

// Aviso sonoro de "pedido nuevo pendiente" cada 10 s, solo mientras haya una pantalla de cocina
// abierta (su latido llega a /Cocina/Ping). Suena en el equipo donde corre la app.
builder.Services.AddSingleton<ELRINCONDORADO.Services.PantallaCocinaTracker>();
builder.Services.AddHostedService<ELRINCONDORADO.Services.AvisoPedidosCocinaService>();

// El POS del Cajero envía el token anti-falsificación como CABECERA (RequestVerificationToken)
// con Content-Type application/json. Por defecto .NET solo acepta el token como campo de formulario,
// así que hay que decirle explícitamente que lo lea de esa cabecera, o el POST Facturar responde 400.
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

// El formato numérico del SO (p. ej. es-BO) usa "." como separador de miles y "," como decimal;
// los <input type="number"> siempre envían el valor canónico con punto, así que el binding lo
// interpretaba mal (0.05 -> 5). Se fija una cultura con decimal "." preservando el formato de fecha.
var cultura = new CultureInfo("es-VE", false);
var numFmt = (NumberFormatInfo)cultura.NumberFormat.Clone();
numFmt.NumberDecimalSeparator = ".";
numFmt.NumberGroupSeparator = ",";
cultura.NumberFormat = numFmt;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(cultura);
    options.SupportedCultures = new[] { cultura };
    options.SupportedUICultures = new[] { cultura };
    options.RequestCultureProviders.Clear();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Aplicar la cultura fija (decimal ".") a todas las peticiones
app.UseRequestLocalization();

// Usar sesiones
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<ELRINCONDORADO.Hubs.PedidosHub>("/pedidosHub");

app.Run();
