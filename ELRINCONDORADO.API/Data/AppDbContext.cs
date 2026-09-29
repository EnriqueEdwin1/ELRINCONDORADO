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
    public DbSet<ConfiguracionFila> Configuracion { get; set; }

    // Módulo Categorías (migrado). Las demás tablas se incorporan módulo a módulo.
    public DbSet<Entities.Categoria> Categorias { get; set; }
    public DbSet<Entities.Producto> Productos { get; set; }

    // Módulo Insumos (migrado).
    public DbSet<Entities.Insumo> Insumos { get; set; }
    public DbSet<Entities.DestinoInsumo> DestinosInsumos { get; set; }

    // Módulo Recetas (migrado).
    public DbSet<Entities.Receta> Recetas { get; set; }
    public DbSet<Entities.DetalleReceta> DetalleRecetas { get; set; }

    // Módulo Movimientos de inventario (migrado).
    public DbSet<Entities.MovimientoInventario> MovimientosInventario { get; set; }

    // Módulo Promociones (migrado).
    public DbSet<Entities.Promocion> Promociones { get; set; }
    public DbSet<Entities.DetallePromocion> DetallePromociones { get; set; }

    // Módulo Empleados y Roles (migrado).
    public DbSet<Entities.Rol> Roles { get; set; }
    public DbSet<Entities.Empleado> Empleados { get; set; }

    // Módulo Mesas (migrado).
    public DbSet<Entities.Mesa> Mesas { get; set; }

    // Módulo Pedidos y ventas (migrado).
    public DbSet<Entities.Cliente> Clientes { get; set; }
    public DbSet<Entities.Pedido> Pedidos { get; set; }
    public DbSet<Entities.DetallePedido> DetallePedidos { get; set; }

    // Módulo Proveedores y compras (migrado).
    public DbSet<Entities.Proveedor> Proveedores { get; set; }
    public DbSet<Entities.Compra> Compras { get; set; }
    public DbSet<Entities.DetalleCompra> DetalleCompras { get; set; }

    // Módulo Cierres de caja (migrado). Última de las 20 tablas de Supabase.
    public DbSet<Entities.CierreCaja> CierresCaja { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mapeo contra el esquema existente: tabla 'configuracion', PK 'clave'.
        modelBuilder.Entity<ConfiguracionFila>().ToTable("configuracion");
        modelBuilder.Entity<ConfiguracionFila>().HasKey(e => e.Clave);

        // ===== Módulo Categorías: se replica el mapeo del DbContext del MVC =====
        modelBuilder.Entity<Entities.Categoria>().ToTable("categorias");
        modelBuilder.Entity<Entities.Categoria>().HasKey(e => e.IdCategoria);

        // ===== Módulo Productos =====
        modelBuilder.Entity<Entities.Producto>().ToTable("productos");
        modelBuilder.Entity<Entities.Producto>().HasKey(e => e.IdProducto);

        // Misma FK que el MVC: productos.id_categoria -> categorias.id_categoria
        modelBuilder.Entity<Entities.Producto>()
            .HasOne(p => p.Categoria)
            .WithMany()
            .HasForeignKey(p => p.IdCategoria);

        // La precisión se fija para que coincida con numeric(10,2) de Supabase y
        // no se introduzcan decimales de más al leer o escribir.
        modelBuilder.Entity<Entities.Producto>()
            .Property(p => p.Precio)
            .HasPrecision(10, 2);

        // NOTA: 'delete_url' NO se mapea. Es el token de borrado de ImgBB y no debe
        // salir de la API. Ver la nota en Data/Entities/Producto.cs.

        // 1:1 opcional con productos
        modelBuilder.Entity<Entities.Producto>()
            .HasOne(p => p.Receta)
            .WithOne(r => r.Producto)
            .HasForeignKey<Entities.Receta>(r => r.IdProducto);

        // ===== Módulo Insumos =====
        modelBuilder.Entity<Entities.Insumo>().ToTable("insumos");
        modelBuilder.Entity<Entities.Insumo>().HasKey(e => e.IdInsumo);

        modelBuilder.Entity<Entities.DestinoInsumo>().ToTable("destinos_insumos");
        modelBuilder.Entity<Entities.DestinoInsumo>().HasKey(e => e.IdDestino);

        // insumos.id_destino es NULLABLE en Supabase
        modelBuilder.Entity<Entities.Insumo>()
            .HasOne(i => i.Destino)
            .WithMany()
            .HasForeignKey(i => i.IdDestino);

        modelBuilder.Entity<Entities.Insumo>()
            .Property(i => i.CostoUnitario)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Entities.Insumo>()
            .Property(i => i.StockActual)
            .HasPrecision(18, 8);

        modelBuilder.Entity<Entities.Insumo>()
            .Property(i => i.StockMinimo)
            .HasPrecision(18, 8);

        // ===== Módulo Recetas =====
        modelBuilder.Entity<Entities.Receta>().ToTable("recetas");
        modelBuilder.Entity<Entities.Receta>().HasKey(e => e.IdReceta);

        modelBuilder.Entity<Entities.DetalleReceta>().ToTable("detalle_receta");
        modelBuilder.Entity<Entities.DetalleReceta>().HasKey(e => e.IdDetalleReceta);

        modelBuilder.Entity<Entities.DetalleReceta>()
            .HasOne(d => d.Receta)
            .WithMany(r => r.DetalleRecetas)
            .HasForeignKey(d => d.IdReceta);

        modelBuilder.Entity<Entities.DetalleReceta>()
            .HasOne(d => d.Insumo)
            .WithMany(i => i.DetalleRecetas)
            .HasForeignKey(d => d.IdInsumo);

        modelBuilder.Entity<Entities.DetalleReceta>()
            .Property(d => d.Cantidad)
            .HasPrecision(18, 8);

        // ===== Módulo Movimientos de inventario =====
        modelBuilder.Entity<Entities.MovimientoInventario>().ToTable("movimientos_inventario");
        modelBuilder.Entity<Entities.MovimientoInventario>().HasKey(e => e.IdMovimiento);

        modelBuilder.Entity<Entities.MovimientoInventario>()
            .HasOne(m => m.Insumo)
            .WithMany(i => i.Movimientos)
            .HasForeignKey(m => m.IdInsumo);

        modelBuilder.Entity<Entities.MovimientoInventario>()
            .HasOne(m => m.Empleado)
            .WithMany()
            .HasForeignKey(m => m.IdEmpleado);

        modelBuilder.Entity<Entities.MovimientoInventario>()
            .Property(m => m.Cantidad)
            .HasPrecision(18, 8);

        // 'fecha' es 'timestamp without time zone' en Supabase.
        modelBuilder.Entity<Entities.MovimientoInventario>()
            .Property(m => m.Fecha)
            .HasColumnType("timestamp without time zone");

        // 'id_cierre' se deja como int? simple: en Supabase no tiene clave foránea.

        // ===== Módulo Promociones =====
        modelBuilder.Entity<Entities.Promocion>().ToTable("promociones");
        modelBuilder.Entity<Entities.Promocion>().HasKey(e => e.IdPromocion);

        modelBuilder.Entity<Entities.Promocion>()
            .Property(p => p.Valor)
            .HasPrecision(10, 2);

        // En Supabase son 'date', no 'timestamp': se mapean como DateOnly.
        modelBuilder.Entity<Entities.Promocion>()
            .Property(p => p.FechaInicio)
            .HasColumnType("date");

        modelBuilder.Entity<Entities.Promocion>()
            .Property(p => p.FechaFin)
            .HasColumnType("date");

        // NOTA: 'delete_url' NO se mapea, igual que en Producto.

        modelBuilder.Entity<Entities.DetallePromocion>().ToTable("detalle_promociones");
        modelBuilder.Entity<Entities.DetallePromocion>().HasKey(e => e.IdDetallePromocion);

        modelBuilder.Entity<Entities.DetallePromocion>()
            .HasOne(d => d.Promocion)
            .WithMany(p => p.DetallePromociones)
            .HasForeignKey(d => d.IdPromocion);

        modelBuilder.Entity<Entities.DetallePromocion>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.IdProducto);

        // 'cantidad' es 'integer' en Supabase, no numeric.
        modelBuilder.Entity<Entities.DetallePromocion>()
            .Property(d => d.Cantidad)
            .HasColumnType("integer");

        // ===== Módulo Pedidos y ventas =====
        modelBuilder.Entity<Entities.Cliente>().ToTable("clientes");
        modelBuilder.Entity<Entities.Cliente>().HasKey(e => e.IdCliente);
        modelBuilder.Entity<Entities.Cliente>()
            .Property(c => c.Nit).HasMaxLength(20);
        modelBuilder.Entity<Entities.Cliente>()
            .Property(c => c.RazonSocial).HasMaxLength(150);

        modelBuilder.Entity<Entities.Pedido>().ToTable("pedidos");
        modelBuilder.Entity<Entities.Pedido>().HasKey(e => e.IdPedido);

        modelBuilder.Entity<Entities.Pedido>().Property(p => p.Subtotal).HasPrecision(10, 2);
        modelBuilder.Entity<Entities.Pedido>().Property(p => p.Descuento).HasPrecision(10, 2);
        modelBuilder.Entity<Entities.Pedido>().Property(p => p.Total).HasPrecision(10, 2);

        modelBuilder.Entity<Entities.Pedido>().HasOne(p => p.Cliente)
            .WithMany(c => c.Pedidos)
            .HasForeignKey(p => p.IdCliente);

        modelBuilder.Entity<Entities.Pedido>().HasOne(p => p.Mesa)
            .WithMany()
            .HasForeignKey(p => p.IdMesa);

        modelBuilder.Entity<Entities.Pedido>().HasOne(p => p.Empleado)
            .WithMany()
            .HasForeignKey(p => p.IdEmpleado);

        modelBuilder.Entity<Entities.Pedido>().HasOne(p => p.Promocion)
            .WithMany()
            .HasForeignKey(p => p.IdPromocion);

        modelBuilder.Entity<Entities.Pedido>()
            .HasMany(p => p.DetallesPedidos)
            .WithOne(d => d.Pedido)
            .HasForeignKey(d => d.IdPedido);

        modelBuilder.Entity<Entities.DetallePedido>().ToTable("detalle_pedidos");
        modelBuilder.Entity<Entities.DetallePedido>().HasKey(e => e.IdDetalle);

        // 'cantidad' es 'integer' en Supabase.
        modelBuilder.Entity<Entities.DetallePedido>()
            .Property(d => d.Cantidad).HasColumnType("integer");
        modelBuilder.Entity<Entities.DetallePedido>()
            .Property(d => d.PrecioUnitario).HasPrecision(10, 2);
        modelBuilder.Entity<Entities.DetallePedido>()
            .Property(d => d.Subtotal).HasPrecision(10, 2);

        modelBuilder.Entity<Entities.DetallePedido>().HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.IdProducto);

        // NOTA: no existe tabla 'metodos_pago' en Supabase. 'estado_pago' es un
        // varchar libre (EFECTIVO / QR / TARJETA), sin catálogo que lo valide.

        // ===== Módulo Proveedores y compras =====
        // 'proveedores' no tiene 'delete_url' ni claves foráneas en Supabase.
        modelBuilder.Entity<Entities.Proveedor>().ToTable("proveedores");
        modelBuilder.Entity<Entities.Proveedor>().HasKey(e => e.IdProveedor);
        modelBuilder.Entity<Entities.Proveedor>().Property(p => p.Nombre).HasMaxLength(150);
        modelBuilder.Entity<Entities.Proveedor>().Property(p => p.Telefono).HasMaxLength(20);
        modelBuilder.Entity<Entities.Proveedor>().Property(p => p.Direccion).HasMaxLength(255);
        modelBuilder.Entity<Entities.Proveedor>().Property(p => p.Email).HasMaxLength(120);
        modelBuilder.Entity<Entities.Proveedor>().Property(p => p.Estado).HasMaxLength(20);

        modelBuilder.Entity<Entities.Compra>().ToTable("compras");
        modelBuilder.Entity<Entities.Compra>().HasKey(e => e.IdCompra);
        modelBuilder.Entity<Entities.Compra>().Property(c => c.Subtotal).HasPrecision(10, 2);
        modelBuilder.Entity<Entities.Compra>().Property(c => c.Descuento).HasPrecision(10, 2);
        modelBuilder.Entity<Entities.Compra>().Property(c => c.Total).HasPrecision(10, 2);
        modelBuilder.Entity<Entities.Compra>().Property(c => c.Estado).HasMaxLength(20);

        modelBuilder.Entity<Entities.Compra>().HasOne(c => c.Proveedor)
            .WithMany(p => p.Compras)
            .HasForeignKey(c => c.IdProveedor);

        modelBuilder.Entity<Entities.Compra>().HasOne(c => c.Empleado)
            .WithMany()
            .HasForeignKey(c => c.IdEmpleado);

        modelBuilder.Entity<Entities.Compra>()
            .HasMany(c => c.DetalleCompras)
            .WithOne(d => d.Compra)
            .HasForeignKey(d => d.IdCompra);

        modelBuilder.Entity<Entities.DetalleCompra>().ToTable("detalle_compras");
        modelBuilder.Entity<Entities.DetalleCompra>().HasKey(e => e.IdDetalleCompra);

        // OJO: aquí la cantidad es numeric(18,8), a diferencia de detalle_pedidos
        // y detalle_promociones, donde es integer. Los insumos se miden en
        // kilos y litros, que admiten fracciones.
        modelBuilder.Entity<Entities.DetalleCompra>()
            .Property(d => d.Cantidad).HasPrecision(18, 8);
        modelBuilder.Entity<Entities.DetalleCompra>()
            .Property(d => d.CostoUnitario).HasPrecision(10, 2);
        modelBuilder.Entity<Entities.DetalleCompra>()
            .Property(d => d.Subtotal).HasPrecision(10, 2);

        modelBuilder.Entity<Entities.DetalleCompra>().HasOne(d => d.Insumo)
            .WithMany()
            .HasForeignKey(d => d.IdInsumo);

        // ADVERTENCIA DE MODELADO: no existe columna 'id_compra' en
        // 'movimientos_inventario'. Las entradas de stock que genera una compra se
        // vinculan SOLO por texto, con motivo = "COMPRA #<id_compra>". No es una
        // clave foránea y se puede romper en silencio si alguien edita el motivo.
        // Ver Controllers/ComprasController.cs.

        // ===== Módulo Cierres de caja =====
        // 'id_empleado' NO tiene clave foránea en Supabase. La entidad declara la
        // navegación 'Empleado' para poder proyectar el nombre, pero SIN
        // HasForeignKey EF la trataría como una relación independiente e inventaría
        // una columna 'empleado_id_empleado' que no existe (error 42703). Por eso
        // se declara como relación sin FK, que para efectos de JOIN equivale a un
        // LEFT JOIN por id_empleado.
        modelBuilder.Entity<Entities.CierreCaja>().ToTable("cierres_caja");
        modelBuilder.Entity<Entities.CierreCaja>().HasKey(e => e.IdCierre);
        modelBuilder.Entity<Entities.CierreCaja>()
            .Property(c => c.TotalVentas).HasPrecision(10, 2);
        modelBuilder.Entity<Entities.CierreCaja>()
            .HasOne(c => c.Empleado)
            .WithMany()
            .HasForeignKey(c => c.IdEmpleado)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== Módulo Empleados y Roles: se replica el mapeo del DbContext del MVC =====
        modelBuilder.Entity<Entities.Rol>().ToTable("roles");
        modelBuilder.Entity<Entities.Rol>().HasKey(e => e.IdRol);

        modelBuilder.Entity<Entities.Empleado>().ToTable("empleados");
        modelBuilder.Entity<Entities.Empleado>().HasKey(e => e.IdEmpleado);

        // Misma FK que el MVC: empleados.id_rol -> roles.id_rol
        modelBuilder.Entity<Entities.Empleado>()
            .HasOne(e => e.Rol)
            .WithMany()
            .HasForeignKey(e => e.IdRol);

        // En Supabase 'fecha_contratacion' es de tipo date, no timestamp.
        modelBuilder.Entity<Entities.Empleado>()
            .Property(e => e.FechaContratacion)
            .HasColumnType("date");

        // ===== Módulo Mesas: se replica el mapeo del DbContext del MVC =====
        modelBuilder.Entity<Entities.Mesa>().ToTable("mesas");
        modelBuilder.Entity<Entities.Mesa>().HasKey(e => e.IdMesa);

        // NOTA: 'password_hash' NO forma parte de este modelo. La lectura de
        // credenciales la hace AuthDbContext, un contexto aparte.

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
