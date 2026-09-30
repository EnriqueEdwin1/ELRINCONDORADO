namespace ELRINCONDORADO.API.Models.DTOs;

// Listado de promociones. Replica el cálculo de Models/PromocionListadoViewModel.cs
// del MVC: cuánto valen los productos por separado y cuánto se ahorra comprándolos
// dentro de la promoción.
//
// La suma se arma con el PRECIO ACTUAL de cada producto, igual que hace el MVC
// (Promocion.Valor se contrasta contra Producto.Precio). Si sube el precio de un
// producto, el ahorro que muestra la API cambia, igual que en el panel.
//
// 'DeleteUrl' no aparece a propósito: es el token de borrado de ImgBB.
public class PromocionDto
{
    public int IdPromocion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    // PAQUETE
    public string Tipo { get; set; } = string.Empty;

    // Precio de la promoción (columna 'valor' de Supabase).
    public decimal Valor { get; set; }

    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }

    // ACTIVA / INACTIVA
    public string Estado { get; set; } = string.Empty;

    public string? ImagenUrl { get; set; }
    public string? DisplayUrl { get; set; }

    // ===== Derivados del cálculo de ahorro =====

    // Suma de (precio del producto x cantidad) de todos los productos incluidos.
    public decimal SumaProductos { get; set; }

    public int CantidadProductos { get; set; }

    // Ejemplo: "Pollo Broaster economico x3".
    public string? ResumenProductos { get; set; }

    // SumaProductos - Valor. Negativo si la promoción sale más cara que comprar
    // por separado; el MVC impide guardarla así, pero el valor se devuelve tal cual.
    public decimal Ahorro { get; set; }

    // Ahorro como porcentaje, redondeado a 1 decimal. 0 si no hay ahorro real.
    public decimal PorcentajeAhorro { get; set; }

    public bool TieneAhorro { get; set; }

    // ACTIVA y dentro del rango de fechas. El POS (CajeroController) solo ofrece
    // las que tienen Estado == "ACTIVA"; este campo suma además el rango de fechas.
    public bool EstaActiva { get; set; }

    // Productos que abarca la promoción. Se llena en AMBAS respuestas (listado y
    // detalle) porque el listado ya trae nombre, precio y cantidad de cada uno: sin
    // esto, el panel no puede mostrar qué contiene cada promoción.
    public List<PromocionProductoDto> Incluidos { get; set; } = new();
}

// El detalle comparte la forma del listado; se conserva el tipo por claridad.
public class PromocionDetalleDto : PromocionDto
{
}

// Un producto dentro de una promoción.
public class PromocionProductoDto
{
    public int IdProducto { get; set; }
    public string? ProductoNombre { get; set; }
    public decimal ProductoPrecio { get; set; }

    // Un producto puede desactivarse sin quitarlo de la promoción.
    public bool ProductoActivo { get; set; }

    public int Cantidad { get; set; }

    // ProductoPrecio x Cantidad.
    public decimal Subtotal { get; set; }
}
