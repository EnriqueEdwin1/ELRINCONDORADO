using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Data.Auth;

// DbContext separado y dedicado SOLO a leer credenciales.
//
// Existe porque EF Core no admite dos tipos de entidad sobre la misma tabla sin
// una relacion que los una. Al_APP tener su propio contexto, 'password_hash'
// queda fuera del modelo de AppDbContext por completo: los endpoints de listado
// y detalle de empleados ni siquiera pueden seleccionarla.
public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<CredencialesEmpleado> Credenciales { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Misma tabla 'empleados' que usa el proyecto MVC y que AppDbContext
        // mapea como entidad Empleado. Solo cambia la vista del modelo.
        modelBuilder.Entity<CredencialesEmpleado>().ToTable("empleados");
        modelBuilder.Entity<CredencialesEmpleado>().HasKey(e => e.IdEmpleado);
        modelBuilder.Entity<CredencialesEmpleado>()
            .HasOne(e => e.Rol)
            .WithMany()
            .HasForeignKey(e => e.IdRol);

        // La navegacion a Rol arrastra el tipo al modelo de ESTE contexto, asi que
        // hay que mapearla aqui tambien (AppDbContext no comparte modelo).
        modelBuilder.Entity<Entities.Rol>().ToTable("roles");
        modelBuilder.Entity<Entities.Rol>().HasKey(e => e.IdRol);

        base.OnModelCreating(modelBuilder);

        // Misma convencion snake_case del DbContext del MVC.
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

    private static string ToSnakeCase(string name)
    {
        return string.Concat(
            name.Select((c, i) => i > 0 && char.IsUpper(c) ? "_" + c : c.ToString()))
            .ToLowerInvariant();
    }
}
