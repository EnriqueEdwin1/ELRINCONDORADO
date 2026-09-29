namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'detalle_promociones' de Supabase.
// Une qué productos entran en una promoción y en qué cantidad.
//
// Ojo con el tipo: 'cantidad' es 'integer' en Supabase, no numeric. Por eso aquí
// es int y no decimal, a diferencia de las cantidades de las recetas.
public class DetallePromocion
{
    public int IdDetallePromocion { get; set; }
    public int IdPromocion { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }

    public Promocion? Promocion { get; set; }
    public Producto? Producto { get; set; }
}
