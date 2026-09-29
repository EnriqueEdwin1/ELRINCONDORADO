namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'pedidos' de Supabase.
// Cabecera de una venta: qué se pidió, quién lo atendió, cómo se pagó y cuánto
// costó. Las líneas viven en detalle_pedidos.
//
// IMPORTANTE: no existe una tabla 'metodos_pago'. 'estado_pago' es un varchar
// libre y los valores que hay en producción son EFECTIVO, QR y TARJETA. No se
// puede validar contra un catálogo.
//
// 'fecha_creacion' es 'timestamp without time zone' en Supabase y guarda la hora
// local del restaurante, no UTC. Ojo: no convertir a UTC al comparar fechas o los
// pedidos de la tarde se corren de día.
public class Pedido
{
    public int IdPedido { get; set; }

    // NULL en pedidos PARA_LLEVAR.
    public int? IdMesa { get; set; }

    public int IdEmpleado { get; set; }

    // NULL si la venta no aplicó ninguna promoción.
    public int? IdPromocion { get; set; }

    // MESA / PARA_LLEVAR / LOCAL
    public string TipoPedido { get; set; } = string.Empty;

    // PENDIENTE / ENTREGADO / CANCELADO. Ojo: el valor real es CANCELADO, no ANULADO.
    public string Estado { get; set; } = string.Empty;

    // PENDIENTE / EFECTIVO / QR / TARJETA
    public string EstadoPago { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }

    // Los tres importes son numeric(10,2) en Supabase.
    public decimal Subtotal { get; set; }

    // Descuento aplicado, ya incluido en el total.
    public decimal Descuento { get; set; }

    // Total = Subtotal - Descuento. En la base siempre cuadra.
    public decimal Total { get; set; }

    // Nombre del pedido o del cliente, tal como aparece en cocina y en el ticket.
    public string? NombrePedido { get; set; }

    // Número de factura consecutivo. Se reinicia en 1 en cada cierre de caja, así
    // que NO es único: se puede repetir entre cierres.
    public string? NumeroPedido { get; set; }

    // NULL si la venta no se facturó a un cliente.
    public int? IdCliente { get; set; }

    public string? Observaciones { get; set; }

    public Cliente? Cliente { get; set; }
    public Mesa? Mesa { get; set; }
    public Empleado? Empleado { get; set; }
    public Promocion? Promocion { get; set; }
    public ICollection<DetallePedido>? DetallesPedidos { get; set; }
}
