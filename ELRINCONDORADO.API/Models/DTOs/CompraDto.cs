namespace ELRINCONDORADO.API.Models.DTOs;

// Listado de proveedores, con sus totales de compra agregados.
public class ProveedorDto
{
    public int IdProveedor { get; set; }
    public string Nombre { get; set; } = string.Empty;

    // Datos de contacto del proveedor, necesarios para ordenar.
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Email { get; set; }

    // ACTIVO / INACTIVO
    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }

    // ===== Derivados =====

    public int TotalCompras { get; set; }
    public decimal TotalGastado { get; set; }
    public DateTime? UltimaCompra { get; set; }
}

// Listado de compras.
public class CompraDto
{
    public int IdCompra { get; set; }

    public int IdProveedor { get; set; }
    public string? ProveedorNombre { get; set; }

    public int IdEmpleado { get; set; }
    public string? EmpleadoNombre { get; set; }

    // Hora local, sin convertir a UTC.
    public DateTime FechaCompra { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }

    // REGISTRADA (único valor en producción)
    public string Estado { get; set; } = string.Empty;

    // ===== Derivados =====

    public int Lineas { get; set; }
    public decimal SumaDetalle { get; set; }

    // true si el subtotal de la cabecera cuadra con la suma de sus líneas.
    public bool SubtotalConsistente { get; set; }
}

// Detalle de una compra con sus insumos.
public class CompraDetalleDto : CompraDto
{
    public List<CompraItemDto> Items { get; set; } = new();

    // ===== Integridad con el inventario =====
    //
    // No hay clave foránea entre compras y movimientos_inventario: el vínculo es
    // el texto motivo = "COMPRA #<id>". Estos dos campos dicen si ese vínculo
    // sigue funcionando, en lugar de asumirlo.

    // Líneas de la compra cuyo movimiento ENTRADA correspondiente existe.
    public int LineasConEntradaRegistrada { get; set; }

    // Total de líneas. Si es mayor que LineasConEntradaRegistrada, hay insumos
    // comprados que nunca entraron al stock.
    public int LineasTotales { get; set; }

    // true si todas las líneas generaron su entrada de inventario.
    public bool InventarioCompleto { get; set; }
}

// Una línea de compra.
public class CompraItemDto
{
    public int IdDetalleCompra { get; set; }
    public int IdInsumo { get; set; }
    public string? InsumoNombre { get; set; }
    public bool InsumoActivo { get; set; }

    // numeric(18,8): admite fracciones, porque los insumos se miden en kg/litros.
    public decimal Cantidad { get; set; }

    // Precio pagado en esta compra.
    public decimal CostoUnitario { get; set; }
    public decimal Subtotal { get; set; }

    // Costo que tiene el insumo HOY en el catálogo. Permite ver si el precio
    // cambió desde la compra. En producción coincide con CostoUnitario.
    public decimal? CostoUnitarioActual { get; set; }

    public bool CostoCambioDesdeCompra { get; set; }

    // true si existe el movimiento ENTRADA de esta línea (motivo "COMPRA #<id>").
    public bool EntradaRegistrada { get; set; }
}
