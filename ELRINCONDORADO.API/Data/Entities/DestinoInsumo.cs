namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'destinos_insumos' de Supabase.
// Un insumo puede apuntar a un destino (COCINA, MESAS, BEBIDAS...).
public class DestinoInsumo
{
    public int IdDestino { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
