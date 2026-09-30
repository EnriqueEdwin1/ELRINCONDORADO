namespace ELRINCONDORADO.API.Models.DTOs;

// Listado de pedidos. Es la cabecera de la venta; las líneas están en
// detalle_pedidos y solo se incluyen en el detalle (/api/pedidos/{id}).
//
// Privacidad: aquí NO se devuelve el NIT del cliente, solo la razón social. El
// NIT sí aparece en el detalle, que es donde lo necesita la factura.
public class PedidoDto
{
    public int IdPedido { get; set; }

    // MESA / PARA_LLEVAR / LOCAL
    public string TipoPedido { get; set; } = string.Empty;

    // PENDIENTE / ENTREGADO / CANCELADO. Ojo: el valor real es CANCELADO, no ANULADO.
    public string Estado { get; set; } = string.Empty;

    // PENDIENTE / EFECTIVO / QR / TARJETA. Varchar libre: no hay catálogo.
    public string EstadoPago { get; set; } = string.Empty;

    // Hora local del restaurante tal como se guardó. Sin conversión a UTC.
    public DateTime FechaCreacion { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }

    public string? NombrePedido { get; set; }

    // Consecutivo que se reinicia en cada cierre de caja: NO es único.
    public string? NumeroPedido { get; set; }

    public string? Observaciones { get; set; }

    // ===== Relaciones, aplanadas para no obligar al cliente a hacer joins =====

    public int? IdMesa { get; set; }

    // La mesa se identifica por su número, no por un nombre.
    public int? MesaNumero { get; set; }

    public int IdEmpleado { get; set; }
    public string? EmpleadoNombre { get; set; }

    public int? IdPromocion { get; set; }
    public string? PromocionNombre { get; set; }

    public int? IdCliente { get; set; }
    public string? ClienteRazonSocial { get; set; }

    // ===== Derivados =====

    // Número de líneas del pedido.
    public int CantidadItems { get; set; }

    // Suma de los subtotales de las líneas. En producción siempre equals Subtotal.
    public decimal TotalItems { get; set; }

    // true si TotalItems no cuadra con el Subtotal guardado en la cabecera.
    // Sirve como alarma de integridad; hoy sale false en los 23 pedidos.
    public bool SubtotalConsistente { get; set; }
}

// Detalle de un pedido, con sus líneas.
public class PedidoDetalleDto : PedidoDto
{
    public List<PedidoItemDto> Items { get; set; } = new();

    // Datos de facturación. El NIT solo se expone aquí, no en el listado.
    public string? ClienteNit { get; set; }
}

// Una línea del pedido.
public class PedidoItemDto
{
    public int IdDetalle { get; set; }
    public int IdProducto { get; set; }
    public string? ProductoNombre { get; set; }
    public bool ProductoActivo { get; set; }

    public int Cantidad { get; set; }

    // Precio CONGELADO al momento de la venta, no el actual del catálogo.
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public string? Observacion { get; set; }

    // Precio actual del producto en el catálogo. Sirve para detectar que el
    // precio cambió después de la venta; no afecta lo ya facturado.
    public decimal? ProductoPrecioActual { get; set; }

    // true si el precio actual difiere del que se cobró.
    public bool PrecioCambioDesdeVenta { get; set; }
}

// ===== Escritura: el POS del Cajero =====

/// Petición de creación de un pedido (POST /api/pedidos).
public class CrearPedidoRequest
{
    public string? NombrePedido { get; set; }

    // Si viene, se hace upsert del cliente por NIT.
    public string? Nit { get; set; }
    public string? RazonSocial { get; set; }

    // "PARA_LLEVAR" o "MESA".
    public string? TipoPedido { get; set; }

    // Solo se guarda si TipoPedido es MESA.
    public int? IdMesa { get; set; }

    // EFECTIVO / QR / TARJETA.
    public string? MetodoPago { get; set; }

    public string? Observaciones { get; set; }

    public List<CrearPedidoItemRequest> Items { get; set; } = new();
}

/// Una línea del carrito: un producto O una promoción.
public class CrearPedidoItemRequest
{
    public int IdProducto { get; set; }
    public int IdPromocion { get; set; }
    public int Cantidad { get; set; }
    public string? Observacion { get; set; }
}

/// Petición de cambio de estado (PUT /api/pedidos/{id}/estado).
public class CambiarEstadoRequest
{
    public string? Estado { get; set; }
}

// ===== Cola de cocina =====

/// Fila de la cola de cocina (GET /api/pedidos/cola).
public class ColaPedidoDto
{
    public int IdPedido { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string TipoPedido { get; set; } = string.Empty;
    public int? MesaNumero { get; set; }
    public string EstadoPago { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public string? NombrePedido { get; set; }
    public string? NombreCajero { get; set; }
    public List<ColaDetalleDto> Detalle { get; set; } = new();
}

/// Línea resumida para la cola: solo lo que necesita la cocina.
public class ColaDetalleDto
{
    public int Cantidad { get; set; }
    public string? Nombre { get; set; }
    public string? Observacion { get; set; }
}

/// Resultado de la búsqueda de cliente por NIT (GET /api/clientes?nit=).
public class ClienteBusquedaDto
{
    public bool Ok { get; set; }
    public string? RazonSocial { get; set; }
    public bool Bloqueado { get; set; }
    public string? Mensaje { get; set; }
}
