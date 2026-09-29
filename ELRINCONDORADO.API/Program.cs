using ELRINCONDORADO.API.Data;
using Microsoft.EntityFrameworkCore;

// Npgsql 6+ exige UTC para "timestamp with time zone"; las columnas de esta base
// usan "timestamp without time zone". Mismo ajuste que aplica el proyecto MVC,
// necesario para leer las mismas tablas.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// La cadena de conexion se resuelve desde User Secrets en desarrollo
// (ConnectionStrings:DefaultConnection). No se escribe en appsettings.json ni
// en el codigo, para no versionar la contrasena de Supabase.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Falta ConnectionStrings:DefaultConnection. Configuralo con: " +
        "dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<cadena>\" --project ELRINCONDORADO.API");
}

// Se registra la MISMA base de datos que ya usa el MVC. No se ejecuta ninguna
// migracion: el esquema de Supabase se deja intacto.
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
