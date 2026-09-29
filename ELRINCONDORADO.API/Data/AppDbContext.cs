using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Data;

// DbContext de la API. Apunta a la MISMA base de datos Supabase que usa el
// proyecto MVC, pero NUNCA se ejecutan migraciones EF: el esquema de Supabase ya
// existe y se mantiene igual. Este proyecto no referencia
// Microsoft.EntityFrameworkCore.Design justamente para que 'dotnet ef' no pueda
// generar ni aplicar migraciones contra la base de datos.
//
// Por eso OnModelCreating no se usa para crear tablas, solo para LEERLAS: cada
// entidad que se agregue debe mapearse con el nombre real de la tabla y de las
// columnas tal como ya existen en Supabase.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // TEMPORAL: unica tabla expuesta en esta etapa, usada por GET /api/health/database.
    // Las otras 19 se incorporan al migrar cada modulo.
    public DbSet<ConfiguracionFila> Configuracion { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mapeo contra el esquema existente: tabla 'configuracion', PK 'clave'.
        modelBuilder.Entity<ConfiguracionFila>().ToTable("configuracion");
        modelBuilder.Entity<ConfiguracionFila>().HasKey(e => e.Clave);

        base.OnModelCreating(modelBuilder);

        // Misma convencion snake_case que aplica el DbContext del proyecto MVC,
        // para que las columnas mapeadas aqui coincidan con las de Supabase.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tabla = entityType.GetTableName();
            if (!string.IsNullOrEmpty(tabla))
            {
                entityType.SetTableName(ToSnakeCase(tabla));
            }

            foreach (var propiedad in entityType.GetProperties())
            {
                propiedad.SetColumnName(ToSnakeCase(propiedad.Name));
            }
        }
    }

    // Convierte un nombre PascalCase a snake_case, ej: IdEmpleado -> id_empleado
    private static string ToSnakeCase(string name)
    {
        return string.Concat(
            name.Select((c, i) => i > 0 && char.IsUpper(c) ? "_" + c : c.ToString()))
            .ToLowerInvariant();
    }
}
