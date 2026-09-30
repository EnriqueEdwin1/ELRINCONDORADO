namespace ELRINCONDORADO.Models.ApiDtos;

// Espejo de la respuesta real de la API (ELRINCONDORADO.API/Models/DTOs/PedidoDto.cs).
// Importante: los nombres coinciden con lo que devuelve la API (camelCase al
// serializar). El mapeo a los modelos de vista vive en Models/Mappers.cs.

public class PedidoDto
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
    public int? MesaNumero { get; set; }
    public int IdEmpleado { get; set; }
    public string? EmpleadoNombre { get; set; }
    public int? IdPromocion { get; set; }
    public string? PromocionNombre { get; set; }
    public int? IdCliente { get; set; }
    public string? ClienteRazonSocial { get; set; }
    public int CantidadItems { get; set; }
    public decimal TotalItems { get; set; }
    public bool SubtotalConsistente { get; set; }
}

public class PedidoDetalleDto : PedidoDto
{
    public List<PedidoItemDto> Items { get; set; } = new();

    // El NIT solo se expone en el detalle, no en el listado.
    public string? ClienteNit { get; set; }
}

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
    public decimal? ProductoPrecioActual { get; set; }
    public bool PrecioCambioDesdeVenta { get; set; }
}

public class PedidoPaginadoDto
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<PedidoDto> Items { get; set; } = new();
}

// Cliente de facturación. Se conserva porque CierreCajaDto lo usa.
public class ClienteDto
{
    public int IdCliente { get; set; }
    public string? Nit { get; set; }
    public string? RazonSocial { get; set; }
}

// Cola de cocina: lo que la API devuelve en GET /api/pedidos/cola.
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

public class ColaDetalleDto
{
    public int Cantidad { get; set; }
    public string? Nombre { get; set; }
    public string? Observacion { get; set; }
}

// Peticiones de escritura que el MVC reenvía a la API.
public class CrearPedidoRequest
{
    public string? NombrePedido { get; set; }
    public string? Nit { get; set; }
    public string? RazonSocial { get; set; }
    public string? TipoPedido { get; set; }
    public int? IdMesa { get; set; }
    public string? MetodoPago { get; set; }
    public string? Observaciones { get; set; }
    public List<CrearPedidoItemRequest> Items { get; set; } = new();
}

public class CrearPedidoItemRequest
{
    public int IdProducto { get; set; }
    public int IdPromocion { get; set; }
    public int Cantidad { get; set; }
    public string? Observacion { get; set; }
}

public class CrearPedidoResponse
{
    public bool Ok { get; set; }
    public int IdPedido { get; set; }
    public string? Mensaje { get; set; }
}

// Resultado de la búsqueda de cliente por NIT (GET /api/clientes?nit=).
public class ClienteBusquedaDto
{
    public bool Ok { get; set; }
    public string? RazonSocial { get; set; }
    public bool Bloqueado { get; set; }
    public string? Mensaje { get; set; }
}