using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using QuestPDF.Infrastructure;

// Npgsql 6+ exige UTC para "timestamp with time zone"; las columnas usan "timestamp without time zone"
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ELRINCONDORADO.Data.AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Agregar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

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


app.Run();
