namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'insumos' de Supabase.
// Es el inventario de materia prima que consume la cocina.
public class Insumo
{
    public int IdInsumo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;

    // numeric(18,8) en Supabase: los stocks manejan 8 decimales porque se restan
    // cantidades fraccionarias de las recetas (litros, gramos).
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }

    // numeric(10,2) en Supabase
    public decimal CostoUnitario { get; set; }

    public bool Activo { get; set; }

    // Opcional: hay insumos sin destino asignado.
    public int? IdDestino { get; set; }

    public DestinoInsumo? Destino { get; set; }
    public ICollection<DetalleReceta>? DetalleRecetas { get; set; }
    public ICollection<MovimientoInventario>? Movimientos { get; set; }
}
