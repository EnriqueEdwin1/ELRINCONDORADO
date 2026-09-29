namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'detalle_receta' de Supabase.
// Cada fila es un insumo que consume la receta, con su cantidad.
public class DetalleReceta
{
    public int IdDetalleReceta { get; set; }
    public int IdReceta { get; set; }
    public int IdInsumo { get; set; }

    // numeric(18,8) en Supabase
    public decimal Cantidad { get; set; }

    public string UnidadMedida { get; set; } = string.Empty;

    public Receta? Receta { get; set; }
    public Insumo? Insumo { get; set; }
}
