namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'proveedores' de Supabase.
//
// A diferencia de productos y promociones, esta tabla NO tiene columna
// 'delete_url', así que no hay token de ImgBB que ocultar.
//
// 'telefono' y 'email' son datos de contacto del proveedor (persona jurídica o
// natural que vende), no datos personales de empleados. Se exponen porque un
// módulo de compras los necesita para ordenar.
public class Proveedor
{
    public int IdProveedor { get; set; }

    // varchar(150)
    public string Nombre { get; set; } = string.Empty;

    // varchar(20), NULL
    public string? Telefono { get; set; }

    // varchar(255), NULL
    public string? Direccion { get; set; }

    // varchar(120), NULL
    public string? Email { get; set; }

    // ACTIVO / INACTIVO. Solo hay 'ACTIVO' en producción.
    public string Estado { get; set; } = string.Empty;

    // text, NULL
    public string? Observaciones { get; set; }

    public ICollection<Compra>? Compras { get; set; }
}
