using System.Text;
using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Auth;
using ELRINCONDORADO.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

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

// Contexto aparte para leer credenciales: mantiene 'password_hash' fuera del
// modelo de AppDbContext.
builder.Services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(connectionString));

// ===== Autenticacion por token (JWT) =====
// La clave de firma vive en User Secrets, nunca en appsettings.json ni en el codigo.
var signingKey = builder.Configuration["Jwt:SigningKey"];
if (string.IsNullOrWhiteSpace(signingKey))
{
    throw new InvalidOperationException(
        "Falta Jwt:SigningKey. Configuralo con: " +
        "dotnet user-secrets set \"Jwt:SigningKey\" \"<clave>\" --project ELRINCONDORADO.API");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// Los endpoints sin [Authorize] siguen abiertos. A partir de aqui, todo endpoint
// de escritura (POST/PUT/DELETE) debe llevar [Authorize] obligatoriamente.
builder.Services.AddAuthorization();

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AutenticacionService>();

// ===== SignalR para comunicación en tiempo real =====
builder.Services.AddSignalR();
builder.Services.AddSingleton<ELRINCONDORADO.API.Services.SignalR.PantallaCocinaTracker>();
builder.Services.AddHostedService<ELRINCONDORADO.API.Services.SignalR.AvisoPedidosCocinaService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ===== CORS: permitir peticiones desde el proyecto MVC =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
    {
        policy.WithOrigins("https://localhost:7210", "http://localhost:5049", "http://localhost:5133", "https://localhost:7167")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega aqui el token que devuelve POST /api/auth/login."
    });

    c.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", documento, null!)] = new List<string>()
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Orden segun el template oficial de ASP.NET Core: la redireccion a HTTPS
// ocurre antes de procesar la autenticacion.
app.UseHttpsRedirection();

// CORS debe ir antes de Authentication/Authorization
app.UseCors("AllowMVC");

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();
app.MapHub<ELRINCONDORADO.API.Hubs.PedidosHub>("/pedidosHub");

app.Run();
