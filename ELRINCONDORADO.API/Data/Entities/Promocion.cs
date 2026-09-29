namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'promociones' de Supabase.
// Una promoción agrupa varios productos (detalle_promociones) y se ofrece a un
// precio fijo 'valor'. En la práctica solo se usa el tipo PAQUETE.
//
// IMPORTANTE: la columna 'delete_url' NO se mapea, por el mismo motivo que en
// Producto: es el token de borrado de ImgBB y no debe salir de la API.
//
// Ojo con los tipos: 'fecha_inicio' y 'fecha_fin' son 'date' en Supabase, NO
// timestamp. Se mapean como DateOnly para no inventar una hora que no existe.
public class Promocion
{
    public int IdPromocion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    // PAQUETE
    public string Tipo { get; set; } = string.Empty;

    // Precio de la promoción. numeric(10,2) en Supabase.
    public decimal Valor { get; set; }

    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }

    // Franja horaria opcional: 'time without time zone' en Supabase.
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }

    // ACTIVA / INACTIVA
    public string Estado { get; set; } = string.Empty;

    public string? ImagenUrl { get; set; }
    public string? DisplayUrl { get; set; }

    public ICollection<DetallePromocion>? DetallePromociones { get; set; }
}
