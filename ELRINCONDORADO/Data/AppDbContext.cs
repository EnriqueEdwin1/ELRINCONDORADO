using Microsoft.EntityFrameworkCore;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Promocion> Promociones { get; set; }
        public DbSet<DetallePromocion> DetallePromociones { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedidos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<Insumo> Insumos { get; set; }
        public DbSet<DestinoInsumo> DestinosInsumos { get; set; }
        public DbSet<Receta> Recetas { get; set; }
        public DbSet<DetalleReceta> DetalleRecetas { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetallesCompras { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<Configuracion> Configuracion { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();

                optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== Claves primarias (convención usa Id o {Nombre}Id, estos modelos usan Id{n} ) =====
            modelBuilder.Entity<Rol>().HasKey(e => e.IdRol);
            modelBuilder.Entity<Empleado>().HasKey(e => e.IdEmpleado);
            modelBuilder.Entity<Mesa>().HasKey(e => e.IdMesa);
            modelBuilder.Entity<Categoria>().HasKey(e => e.IdCategoria);
            modelBuilder.Entity<Producto>().HasKey(e => e.IdProducto);
            modelBuilder.Entity<Promocion>().HasKey(e => e.IdPromocion);
            modelBuilder.Entity<DetallePromocion>().HasKey(e => e.IdDetallePromocion);
            modelBuilder.Entity<Pedido>().HasKey(e => e.IdPedido);
            modelBuilder.Entity<Cliente>().HasKey(e => e.IdCliente);
            modelBuilder.Entity<DetallePedido>().HasKey(e => e.IdDetalle);
            modelBuilder.Entity<Venta>().HasKey(e => e.IdVenta);
            modelBuilder.Entity<Insumo>().HasKey(e => e.IdInsumo);
            modelBuilder.Entity<DestinoInsumo>().HasKey(e => e.IdDestino);
            modelBuilder.Entity<Receta>().HasKey(e => e.IdReceta);
            modelBuilder.Entity<DetalleReceta>().HasKey(e => e.IdDetalleReceta);
            modelBuilder.Entity<Proveedor>().HasKey(e => e.IdProveedor);
            modelBuilder.Entity<Compra>().HasKey(e => e.IdCompra);
            modelBuilder.Entity<DetalleCompra>().HasKey(e => e.IdDetalleCompra);
            modelBuilder.Entity<MovimientoInventario>().HasKey(e => e.IdMovimiento);
            modelBuilder.Entity<Configuracion>().HasKey(e => e.Clave);

            // Configurar relación 1:1 Producto - Receta
            modelBuilder.Entity<Receta>()
                .HasOne(r => r.Producto)
                .WithOne(p => p.Receta)
                .HasForeignKey<Receta>(r => r.IdProducto);

            // ===== Relaciones uno a muchos (FK explícitas) =====
            modelBuilder.Entity<Empleado>()
                .HasOne(e => e.Rol)
                .WithMany(r => r.Empleados)
                .HasForeignKey(e => e.IdRol);
            modelBuilder.Entity<Empleado>()
                .Property(e => e.FechaContratacion)
                .HasColumnType("date");

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.IdCategoria);

            modelBuilder.Entity<DetallePromocion>()
                .HasOne(d => d.Promocion)
                .WithMany(p => p.DetallePromociones)
                .HasForeignKey(d => d.IdPromocion);
            modelBuilder.Entity<DetallePromocion>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallePromociones)
                .HasForeignKey(d => d.IdProducto);

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Mesa)
                .WithMany(m => m.Pedidos)
                .HasForeignKey(p => p.IdMesa);
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Empleado)
                .WithMany(e => e.Pedidos)
                .HasForeignKey(p => p.IdEmpleado);
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Promocion)
                .WithMany(p => p.Pedidos)
                .HasForeignKey(p => p.IdPromocion);
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.IdCliente);

            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.DetallesPedidos)
                .HasForeignKey(d => d.IdPedido);
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesPedidos)
                .HasForeignKey(d => d.IdProducto);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Mesa)
                .WithMany(m => m.Ventas)
                .HasForeignKey(v => v.IdMesa);
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Empleado)
                .WithMany(e => e.Ventas)
                .HasForeignKey(v => v.IdEmpleado);

            modelBuilder.Entity<DetalleReceta>()
                .HasOne(d => d.Receta)
                .WithMany(r => r.DetalleRecetas)
                .HasForeignKey(d => d.IdReceta);
            modelBuilder.Entity<DetalleReceta>()
                .HasOne(d => d.Insumo)
                .WithMany(i => i.DetalleRecetas)
                .HasForeignKey(d => d.IdInsumo);

            modelBuilder.Entity<Insumo>()
                .HasOne(i => i.Destino)
                .WithMany(d => d.Insumos)
                .HasForeignKey(i => i.IdDestino);

            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Proveedor)
                .WithMany(p => p.Compras)
                .HasForeignKey(c => c.IdProveedor);
            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Empleado)
                .WithMany(e => e.Compras)
                .HasForeignKey(c => c.IdEmpleado);

            modelBuilder.Entity<DetalleCompra>()
                .HasOne(d => d.Compra)
                .WithMany(c => c.DetallesCompras)
                .HasForeignKey(d => d.IdCompra);
            modelBuilder.Entity<DetalleCompra>()
                .HasOne(d => d.Insumo)
                .WithMany(i => i.DetallesCompras)
                .HasForeignKey(d => d.IdInsumo);

            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.Insumo)
                .WithMany(i => i.MovimientosInventario)
                .HasForeignKey(m => m.IdInsumo);
            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.Empleado)
                .WithMany(e => e.MovimientosInventario)
                .HasForeignKey(m => m.IdEmpleado);

            // ===== Nombres de tabla reales en la BD (todas en plural, snake_case) =====
            modelBuilder.Entity<Rol>().ToTable("roles");
            modelBuilder.Entity<Empleado>().ToTable("empleados");
            modelBuilder.Entity<Mesa>().ToTable("mesas");
            modelBuilder.Entity<Categoria>().ToTable("categorias");
            modelBuilder.Entity<Producto>().ToTable("productos");
            modelBuilder.Entity<Promocion>().ToTable("promociones");
            modelBuilder.Entity<DetallePromocion>().ToTable("detalle_promociones");
            modelBuilder.Entity<Pedido>().ToTable("pedidos");
            modelBuilder.Entity<Cliente>().ToTable("clientes");
            modelBuilder.Entity<Venta>().ToTable("ventas");
            modelBuilder.Entity<Insumo>().ToTable("insumos");
            modelBuilder.Entity<DestinoInsumo>().ToTable("destinos_insumos");
            modelBuilder.Entity<Receta>().ToTable("recetas");
            modelBuilder.Entity<Proveedor>().ToTable("proveedores");
            modelBuilder.Entity<Compra>().ToTable("compras");
            modelBuilder.Entity<MovimientoInventario>().ToTable("movimientos_inventario");
            modelBuilder.Entity<DetallePedido>().ToTable("detalle_pedidos");
            modelBuilder.Entity<DetalleReceta>().ToTable("detalle_receta");
            modelBuilder.Entity<DetalleCompra>().ToTable("detalle_compras");
            modelBuilder.Entity<Configuracion>().ToTable("configuracion");

            // Mapeo snake_case para tablas y columnas (la BD usa minúsculas con guion bajo)
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var table = entityType.GetTableName();
                if (!string.IsNullOrEmpty(table))
                {
                    entityType.SetTableName(ToSnakeCase(table));
                }

                foreach (var property in entityType.GetProperties())
                {
                    property.SetColumnName(ToSnakeCase(property.Name));
                }
            }

            // Configurar tabla Productos
            modelBuilder.Entity<Producto>()
                .Property(p => p.Precio)
                .HasColumnType("numeric(10,2)");

            // Configurar tabla Promociones
            modelBuilder.Entity<Promocion>()
                .Property(p => p.Valor)
                .HasColumnType("numeric(10,2)");

            // Configurar tabla Pedidos
            modelBuilder.Entity<Pedido>()
                .Property(p => p.Subtotal)
                .HasColumnType("numeric(10,2)");
            modelBuilder.Entity<Pedido>()
                .Property(p => p.Descuento)
                .HasColumnType("numeric(10,2)");
            modelBuilder.Entity<Pedido>()
                .Property(p => p.Total)
                .HasColumnType("numeric(10,2)");

            // Configurar tabla DetallePedidos
            modelBuilder.Entity<DetallePedido>()
                .Property(d => d.PrecioUnitario)
                .HasColumnType("numeric(10,2)");
            modelBuilder.Entity<DetallePedido>()
                .Property(d => d.Subtotal)
                .HasColumnType("numeric(10,2)");

            // Configurar tabla Ventas
            modelBuilder.Entity<Venta>()
                .Property(v => v.Subtotal)
                .HasColumnType("numeric(10,2)");
            modelBuilder.Entity<Venta>()
                .Property(v => v.Descuento)
                .HasColumnType("numeric(10,2)");
            modelBuilder.Entity<Venta>()
                .Property(v => v.Total)
                .HasColumnType("numeric(10,2)");

            // Configurar tabla Insumos
            modelBuilder.Entity<Insumo>()
                .Property(i => i.StockActual)
                .HasColumnType("numeric(12,3)");
            modelBuilder.Entity<Insumo>()
                .Property(i => i.StockMinimo)
                .HasColumnType("numeric(12,3)");
            modelBuilder.Entity<Insumo>()
                .Property(i => i.CostoUnitario)
                .HasColumnType("numeric(10,2)");

            // Configurar relación 1:1 Producto - Receta
            modelBuilder.Entity<Receta>()
                .HasOne(r => r.Producto)
                .WithOne(p => p.Receta)
                .HasForeignKey<Receta>(r => r.IdProducto);

            // Configurar tabla DetalleReceta
            modelBuilder.Entity<DetalleReceta>()
                .Property(d => d.Cantidad)
                .HasColumnType("numeric(12,3)");

            // Configurar tabla Compras
            modelBuilder.Entity<Compra>()
                .Property(c => c.Subtotal)
                .HasColumnType("numeric(10,2)");
            modelBuilder.Entity<Compra>()
                .Property(c => c.Descuento)
                .HasColumnType("numeric(10,2)");
            modelBuilder.Entity<Compra>()
                .Property(c => c.Total)
                .HasColumnType("numeric(10,2)");

            // Configurar tabla DetalleCompras
            modelBuilder.Entity<DetalleCompra>()
                .Property(d => d.Cantidad)
                .HasColumnType("numeric(12,3)");
            modelBuilder.Entity<DetalleCompra>()
                .Property(d => d.CostoUnitario)
                .HasColumnType("numeric(10,2)");
            modelBuilder.Entity<DetalleCompra>()
                .Property(d => d.Subtotal)
                .HasColumnType("numeric(10,2)");

            // Configurar tabla MovimientosInventario
            modelBuilder.Entity<MovimientoInventario>()
                .Property(m => m.Cantidad)
                .HasColumnType("numeric(12,3)");
        }

        // Convierte un nombre PascalCase a snake_case, ej: IdEmpleado -> id_empleado
        private static string ToSnakeCase(string name)
        {
            return string.Concat(
                name.Select((c, i) => i > 0 && char.IsUpper(c) ? "_" + c : c.ToString()))
                .ToLowerInvariant();
        }
    }
}
