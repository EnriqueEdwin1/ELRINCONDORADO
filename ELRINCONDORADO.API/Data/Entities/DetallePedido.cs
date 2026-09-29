namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'detalle_pedidos' de Supabase.
// Una línea de la venta: qué producto, cuántos y a qué precio.
//
// Ojo con los tipos: 'cantidad' es 'integer' en Supabase, no numeric. Y
// 'precio_unitario' y 'subtotal' son numeric(10,2), es decir el precio queda
// CONGELADO en el momento de la venta. No se recalcula con el precio actual del
// producto, a diferencia de las promociones.
public class DetallePedido
{
    public int IdDetalle { get; set; }
    public int IdPedido { get; set; }
    public int IdProducto { get; set; }

    // integer en Supabase.
    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string? Observacion { get; set; }

    public Pedido? Pedido { get; set; }
    public Producto? Producto { get; set; }
}
