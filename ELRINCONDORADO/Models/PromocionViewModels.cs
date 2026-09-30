namespace ELRINCONDORADO.Models;

// Formulario de alta/edición de una promoción.
//
// 'Incluidos' no son solo los productos marcados: la vista pinta el catálogo
// completo con checkboxes, y 'Seleccionado' + 'Cantidad' marcan los que ya están
// en la promoción. Por eso la carga se arma cruzando el catálogo con 'Incluidos'
// que devuelve la API.
public class PromocionFormViewModel
{
    public int IdPromocion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Tipo { get; set; } = "PAQUETE";
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? Estado { get; set; }
    public bool EsEdicion { get; set; }
    public decimal Precio { get; set; }
    public decimal Ahorro { get; set; }
    public decimal PorcentajeAhorro { get; set; }
    public string? ImagenActual { get; set; }
    public string? DisplayUrlActual { get; set; }
    public List<Producto> Incluidos { get; set; } = new();
    public decimal SumaProductos { get; set; }
}

// Detalle de una promoción con los productos que incluye.
public class PromocionDetalleViewModel
{
    public int IdPromocion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? Estado { get; set; }
    public bool EstaActiva { get; set; }
    public List<Producto> Incluidos { get; set; } = new();
    public decimal SumaProductos { get; set; }
    public decimal Ahorro { get; set; }
    public bool TieneAhorro { get; set; }
    public decimal PorcentajeAhorro { get; set; }
    public decimal Precio { get; set; }
    public string? ImagenUrl { get; set; }
    public int CantidadProductos { get; set; }
    public string? ResumenProductos { get; set; }
}

// Fila del listado de promociones.
public class PromocionListadoViewModel
{
    public int IdPromocion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? Estado { get; set; }
    public bool EstaActiva { get; set; }
    public bool TieneAhorro { get; set; }
    public decimal PorcentajeAhorro { get; set; }
    public decimal Ahorro { get; set; }
    public int CantidadProductos { get; set; }
    public string? ResumenProductos { get; set; }
    public decimal SumaProductos { get; set; }
    public decimal Precio { get; set; }
    public string? ImagenUrl { get; set; }
    public List<PromocionListadoItem> Incluidos { get; set; } = new();
}

// Un producto dentro del listado de promociones.
public class PromocionListadoItem
{
    public int IdProducto { get; set; }
    public string? ProductoNombre { get; set; }
    public decimal ProductoPrecio { get; set; }
    public bool ProductoActivo { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}
