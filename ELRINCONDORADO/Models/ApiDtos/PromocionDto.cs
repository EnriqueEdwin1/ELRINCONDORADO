namespace ELRINCONDORADO.Models.ApiDtos;

// Espejo exacto de ELRINCONDORADO.API/Models/DTOs/PromocionDto.cs.
//
// Antes este DTO declaraba 'Detalles' y 'DetallePromociones', nombres que la API
// nunca devuelve: por eso las promociones llegaban sin productos. La API responde
// con 'Incluidos' y con los derivados del ahorro ya calculados.
public class PromocionDto
{
    public int IdPromocion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    // PAQUETE
    public string Tipo { get; set; } = string.Empty;

    // Precio de la promoción.
    public decimal Valor { get; set; }

    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }

    // ACTIVA / INACTIVA
    public string Estado { get; set; } = string.Empty;

    public string? ImagenUrl { get; set; }
    public string? DisplayUrl { get; set; }

    // ===== Derivados del cálculo de ahorro (los calcula la API) =====

    public decimal SumaProductos { get; set; }
    public int CantidadProductos { get; set; }
    public string? ResumenProductos { get; set; }
    public decimal Ahorro { get; set; }
    public decimal PorcentajeAhorro { get; set; }
    public bool TieneAhorro { get; set; }
    public bool EstaActiva { get; set; }

    // Productos que abarca la promoción.
    public List<PromocionProductoDto> Incluidos { get; set; } = new();
}

// Detalle de una promoción. La API ya devuelve la misma forma que el listado.
public class PromocionDetalleDto : PromocionDto
{
}

// Un producto dentro de una promoción.
public class PromocionProductoDto
{
    public int IdProducto { get; set; }
    public string? ProductoNombre { get; set; }
    public decimal ProductoPrecio { get; set; }
    public bool ProductoActivo { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}

// Espejo de ELRINCONDORADO.API/Data/Dtos/CrearPromocionRequest.cs. Lo usan los
// POST/PUT: la API no espera 'Incluidos' sino 'DetallePromociones'.
public class PromocionRequestDto
{
    public int? IdPromocion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Tipo { get; set; } = "PAQUETE";
    public decimal Valor { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }
    public string Estado { get; set; } = "ACTIVA";
    public string? ImagenUrl { get; set; }
    public string? DisplayUrl { get; set; }
    public List<DetallePromocionRequestDto>? DetallePromociones { get; set; }
}

public class DetallePromocionRequestDto
{
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
}
