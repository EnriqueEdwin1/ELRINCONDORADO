using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Models;

// Los modelos de promociones viven en Models/PromocionViewModels.cs.

public class MovimientosAdminViewModel
{
    public List<MovimientoInventario> Movimientos { get; set; } = new();
    public int CantidadTurnoActual { get; set; }
    public List<MovimientoInventario> TurnoActual { get; set; } = new();
    public List<CierreCaja> Cierres { get; set; } = new();
}

public class MovimientoInventario
{
    public int IdMovimiento { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public DateTime Fecha { get; set; }
    public string? Motivo { get; set; }
    public int IdInsumo { get; set; }
    public string? InsumoNombre { get; set; }
    public int IdEmpleado { get; set; }
    public string? EmpleadoNombre { get; set; }
    public int? IdCierre { get; set; }
    public Insumo? Insumo { get; set; }
    public Empleado? Empleado { get; set; }
}

public class CierreCaja
{
    public int IdCierre { get; set; }
    public DateTime Fecha { get; set; }
    public int IdEmpleado { get; set; }
    public string? EmpleadoNombre { get; set; }
    public decimal TotalVentas { get; set; }
    public int MovimientosCerrados { get; set; }
    public bool EmpleadoDesconocido { get; set; }
    public Empleado? Empleado { get; set; }
    public List<MovimientoInventario> Movimientos { get; set; } = new();
}

public class ProductoRecetaViewModel
{
    public int IdProducto { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; }
    public string? ImagenUrl { get; set; }
    public string? DisplayUrl { get; set; }
    public int IdCategoria { get; set; }
    public string? CategoriaNombre { get; set; }
    public bool TieneReceta { get; set; }
    public List<DetalleRecetaViewModel> Detalles { get; set; } = new();
    public Producto? Producto { get; set; }
    public bool IncluirReceta { get; set; }
    public Receta? Receta { get; set; }
}

public class DetalleRecetaViewModel
{
    public int IdDetalleReceta { get; set; }
    public int IdInsumo { get; set; }
    public string? InsumoNombre { get; set; }
    public decimal Cantidad { get; set; }
    public string? UnidadMedida { get; set; }
}

public class RecetaViewModel
{
    public int IdReceta { get; set; }
    public int IdProducto { get; set; }
    public string? ProductoNombre { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public List<DetalleRecetaViewModel> Detalles { get; set; } = new();
}

public class CompraViewModel
{
    public int IdCompra { get; set; }
    public int IdProveedor { get; set; }
    public string? ProveedorNombre { get; set; }
    public int IdEmpleado { get; set; }
    public string? EmpleadoNombre { get; set; }
    public DateTime FechaCompra { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public List<DetalleCompra> Detalles { get; set; } = new();
}

public class DetalleCompra
{
    public int IdDetalleCompra { get; set; }
    public int IdInsumo { get; set; }
    public string? InsumoNombre { get; set; }
    public bool InsumoActivo { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public decimal? CostoUnitarioActual { get; set; }
    public bool CostoCambioDesdeCompra { get; set; }
    public bool EntradaRegistrada { get; set; }
    public Insumo? Insumo { get; set; }
}

public class AdminResumenViewModel
{
    public CajaResumenDto? CajaResumen { get; set; }
    public List<InsumoDto> InsumosBajoMinimo { get; set; } = new();
    public DateTime? UltimoCierreLocal { get; set; }
    public decimal VendidoHoy { get; set; }
    public int VentasHoy { get; set; }
    public decimal VendidoTurno { get; set; }
    public int VentasTurno { get; set; }
}

public class CajeroIndexViewModel
{
    public List<ProductoDto> Productos { get; set; } = new();
    public List<MesaDto> Mesas { get; set; } = new();
    public List<CategoriaDto> Categorias { get; set; } = new();
    public List<PromocionDto> Promociones { get; set; } = new();
}

public class CierreCajaViewModel
{
    public CajaResumenDto? Resumen { get; set; }
    public decimal Efectivo { get; set; }
    public decimal Tarjeta { get; set; }
    public decimal QR { get; set; }
    public decimal Total { get; set; }
    public List<CierreCajaVentaDto> Ventas { get; set; } = new();
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
    public string? Cajero { get; set; }
    public DateTime? Fecha { get; set; }
}

public class VentasListaViewModel
{
    public List<Pedido> Pedidos { get; set; } = new();
    public int Total { get; set; }
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }
    public decimal Descuento { get; set; }
    public int Cancelados { get; set; }
    public string? Periodo { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}

public class Pedido
{
    public int IdPedido { get; set; }
    public string TipoPedido { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public string? NombrePedido { get; set; }
    public string? NumeroPedido { get; set; }
    public string? Observaciones { get; set; }
    public int? IdMesa { get; set; }
    public int? NumeroMesa { get; set; }
    public int IdEmpleado { get; set; }
    public string? NombreCajero { get; set; }
    public int? IdPromocion { get; set; }
    public string? NombrePromocion { get; set; }
    public int? IdCliente { get; set; }
    public string? NitCliente { get; set; }
    public string? RazonSocialCliente { get; set; }
    public List<DetallePedido> DetallesPedidos { get; set; } = new();
    public Cliente? Cliente { get; set; }
    public Empleado? Empleado { get; set; }
    public Mesa? Mesa { get; set; }
    public PromocionListadoViewModel? Promocion { get; set; }
}

public class Mesa
{
    public int IdMesa { get; set; }
    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class Cliente
{
    public int IdCliente { get; set; }
    public string? Nit { get; set; }
    public string? RazonSocial { get; set; }
}

public class DetallePedido
{
    public int IdDetallePedido { get; set; }
    public int IdDetalle { get => IdDetallePedido; }
    public int IdPedido { get; set; }
    public int IdProducto { get; set; }
    public string? NombreProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string? Observacion { get; set; }
    public Producto? Producto { get; set; }
}

public class Empleado
{
    public int IdEmpleado { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public DateTime? FechaContratacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int IdRol { get; set; }
    public string RolNombre { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public Rol? Rol { get; set; }
}

public class Rol
{
    public int IdRol { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CantidadEmpleados { get; set; }
}

public class DestinoInsumo
{
    public int IdDestino { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public List<Insumo> Insumos { get; set; } = new();
}

public class Insumo
{
    public int IdInsumo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal CostoUnitario { get; set; }
    public bool Activo { get; set; }
    public int? IdDestino { get; set; }
    public string? DestinoNombre { get; set; }
    public bool BajoMinimo { get; set; }
    public List<DetalleCompra> DetallesCompras { get; set; } = new();
    public DestinoInsumo? Destino { get; set; }
    public List<DetalleReceta> DetalleRecetas { get; set; } = new();
}

public class Categoria
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CantidadProductos { get; set; }
    public List<Producto> Productos { get; set; } = new();
}

public class Producto
{
    public int IdProducto { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; }
    public string? ImagenUrl { get; set; }
    public string? DisplayUrl { get; set; }
    public int IdCategoria { get; set; }
    public string? CategoriaNombre { get; set; }
    public bool TieneReceta { get; set; }
    public Categoria? Categoria { get; set; }
    public Receta? Receta { get; set; }
    public bool Seleccionado { get; set; }
    public int Cantidad { get; set; }
}

public class Receta
{
    public int IdReceta { get; set; }
    public int IdProducto { get; set; }
    public string? ProductoNombre { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public List<DetalleReceta> Detalles { get; set; } = new();
    public List<DetalleReceta> DetalleRecetas { get; set; } = new();
    public Producto? Producto { get; set; }
}

public class DetalleReceta
{
    public int IdDetalleReceta { get; set; }
    public int IdInsumo { get; set; }
    public string? InsumoNombre { get; set; }
    public decimal Cantidad { get; set; }
    public string? UnidadMedida { get; set; }
    public Insumo? Insumo { get; set; }
}

public class Compra
{
    public int IdCompra { get; set; }
    public int IdProveedor { get; set; }
    public string? ProveedorNombre { get; set; }
    public int IdEmpleado { get; set; }
    public string? EmpleadoNombre { get; set; }
    public DateTime FechaCompra { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int Lineas { get; set; }
    public decimal SumaDetalle { get; set; }
    public bool SubtotalConsistente { get; set; }
    public List<DetalleCompra> DetallesCompras { get; set; } = new();
    public Empleado? Empleado { get; set; }
    public Proveedor? Proveedor { get; set; }
}

public class Proveedor
{
    public int IdProveedor { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Email { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public int TotalCompras { get; set; }
    public decimal TotalGastado { get; set; }
    public DateTime? UltimaCompra { get; set; }
    public List<Compra> Compras { get; set; } = new();
}

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

public class LoginViewModel
{
    public string Usuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
