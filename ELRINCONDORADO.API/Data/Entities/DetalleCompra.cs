namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'detalle_compras' de Supabase.
// Una línea de compra: qué insumo, cuánto y a qué costo.
//
// OJO CON EL TIPO: aquí 'cantidad' es numeric(18,8), NO integer. Es la única de
// las tres tablas de detalle donde la cantidad es decimal, y es a propósito:
// los insumos se manejan en kilos y litros, que no son números enteros.
// Compárese con detalle_pedidos.cantidad y detalle_promociones.cantidad, que sí
// son integer.
//
// Esta tabla NO tiene columna 'observacion'.
public class DetalleCompra
{
    public int IdDetalleCompra { get; set; }
    public int IdCompra { get; set; }
    public int IdInsumo { get; set; }

    // numeric(18,8) en Supabase: admite 5,5 kilos o 2,25 litros.
    public decimal Cantidad { get; set; }

    // numeric(10,2). En producción coincide siempre con insumos.costo_unitario.
    public decimal CostoUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public Compra? Compra { get; set; }
    public Insumo? Insumo { get; set; }
}
