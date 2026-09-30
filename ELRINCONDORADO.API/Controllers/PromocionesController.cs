using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

// Módulo Promociones. Réplica en solo lectura de la pestaña 'promociones' de
// Controllers/ProductosController.cs del MVC y de la lista de ofertas que usa el
// POS en CajeroController.
//
// Igual que el MVC, la suma y el ahorro se calculan en memoria con el precio
// actual de cada producto, y no con un precio congelado en la promoción.
//
// La columna 'delete_url' nunca sale de aquí: ver Data/Entities/Promocion.cs.
[ApiController]
[Route("api/promociones")]
public class PromocionesController : ControllerBase
{
    private readonly AppDbContext _db;

    public PromocionesController(AppDbContext db)
    {
        _db = db;
    }

    // Equivale a ConstruirListadoPromocionesAsync() del MVC, ordenada por id
    // descendente para dejar las más recientes primero.
    //
    // Filtros opcionales:
    //   ?estado=ACTIVA     solo activas (lo que aplica el POS del cajero)
    //   ?tipo=PAQUETE      por tipo
    //   ?vigentes=true     solo las que están dentro de su rango de fechas
    //   ?buscar=3x2        busca en nombre y descripción
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PromocionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PromocionDto>>> GetAll(
        [FromQuery] string? estado,
        [FromQuery] string? tipo,
        [FromQuery] bool? vigentes,
        [FromQuery] string? buscar,
        CancellationToken ct)
    {
        var consulta = _db.Promociones.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoBuscado = estado.Trim().ToUpperInvariant();
            consulta = consulta.Where(p => p.Estado.ToUpper() == estadoBuscado);
        }

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var tipoBuscado = tipo.Trim().ToUpperInvariant();
            consulta = consulta.Where(p => p.Tipo.ToUpper() == tipoBuscado);
        }

        if (vigentes == true)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            consulta = consulta.Where(p => p.FechaInicio <= hoy && p.FechaFin >= hoy);
        }

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(p =>
                EF.Functions.ILike(p.Nombre, $"%{texto}%") ||
                (p.Descripcion != null && EF.Functions.ILike(p.Descripcion, $"%{texto}%")));
        }

        var promociones = await ConstruirAsync(consulta, ct);

        return Ok(promociones);
    }

    // Equivale a PromocionDetails/{id}: la promoción con el detalle de los
    // productos que incluye.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PromocionDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromocionDetalleDto>> GetById(int id, CancellationToken ct)
    {
        var promociones = await ConstruirAsync(_db.Promociones.AsNoTracking().Where(p => p.IdPromocion == id), ct);

        var promocion = promociones.FirstOrDefault();
        if (promocion is null)
        {
            return NotFound();
        }

        // ConstruirAsync ya deja Incluidos armado, así que no hace falta una
        // segunda consulta: el detalle y el listado devuelven la misma información.
        var detalle = new PromocionDetalleDto
        {
            IdPromocion = promocion.IdPromocion,
            Nombre = promocion.Nombre,
            Descripcion = promocion.Descripcion,
            Tipo = promocion.Tipo,
            Valor = promocion.Valor,
            FechaInicio = promocion.FechaInicio,
            FechaFin = promocion.FechaFin,
            HoraInicio = promocion.HoraInicio,
            HoraFin = promocion.HoraFin,
            Estado = promocion.Estado,
            ImagenUrl = promocion.ImagenUrl,
            DisplayUrl = promocion.DisplayUrl,
            SumaProductos = promocion.SumaProductos,
            CantidadProductos = promocion.CantidadProductos,
            ResumenProductos = promocion.ResumenProductos,
            Ahorro = promocion.Ahorro,
            PorcentajeAhorro = promocion.PorcentajeAhorro,
            TieneAhorro = promocion.TieneAhorro,
            EstaActiva = promocion.EstaActiva,
            Incluidos = promocion.Incluidos
        };

        return Ok(detalle);
    }

    // Materializa las promociones y calcula los derivados con los productos
    // incluidos. Se hace en dos consultas y en memoria, igual que el MVC, para no
    // depender de cómo traduzca SUM y los nulos cada proveedor de EF.
    private async Task<List<PromocionDto>> ConstruirAsync(
        IQueryable<Promocion> consulta, CancellationToken ct)
    {
        var base_ = await consulta
            .OrderByDescending(p => p.IdPromocion)
            .Select(p => new PromocionDto
            {
                IdPromocion = p.IdPromocion,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Tipo = p.Tipo,
                Valor = p.Valor,
                FechaInicio = p.FechaInicio,
                FechaFin = p.FechaFin,
                HoraInicio = p.HoraInicio,
                HoraFin = p.HoraFin,
                Estado = p.Estado,
                ImagenUrl = p.ImagenUrl,
                DisplayUrl = p.DisplayUrl
            })
            .ToListAsync(ct);

        if (base_.Count == 0)
        {
            return base_;
        }

        var ids = base_.Select(p => p.IdPromocion).ToList();

        // Trae solo lo necesario para la aritmética: nombre, precio y cantidad.
        var lineas = await _db.DetallePromociones
            .AsNoTracking()
            .Where(d => ids.Contains(d.IdPromocion))
            .Select(d => new
            {
                d.IdPromocion,
                d.IdProducto,
                Nombre = d.Producto != null ? d.Producto.Nombre : null,
                Precio = d.Producto!.Precio,
                Activo = d.Producto!.Activo,
                d.Cantidad
            })
            .ToListAsync(ct);

        var hoy = DateOnly.FromDateTime(DateTime.Now);

        foreach (var promo in base_)
        {
            var suyas = lineas.Where(l => l.IdPromocion == promo.IdPromocion).ToList();

            // SumaProductos = suma de (precio del producto x cantidad), con el
            // precio actual del producto, tal como hace el MVC.
            promo.SumaProductos = suyas.Sum(l => l.Precio * l.Cantidad);
            promo.CantidadProductos = suyas.Count;

            // "Pollo Broaster economico x3, Papa x2"
            promo.ResumenProductos = suyas.Count == 0
                ? null
                : string.Join(", ", suyas
                    .OrderBy(l => l.Nombre)
                    .Select(l => $"{l.Nombre} x{l.Cantidad}"));

            // El listado también viaja con sus productos, para que el panel y el
            // POS puedan mostrar el contenido de cada promoción sin un GET extra.
            promo.Incluidos = suyas
                .OrderBy(l => l.Nombre)
                .Select(l => new PromocionProductoDto
                {
                    IdProducto = l.IdProducto,
                    ProductoNombre = l.Nombre,
                    ProductoPrecio = l.Precio,
                    ProductoActivo = l.Activo,
                    Cantidad = l.Cantidad,
                    Subtotal = l.Precio * l.Cantidad
                })
                .ToList();

            promo.Ahorro = promo.SumaProductos - promo.Valor;

            // Misma regla del MVC: sin suma o sin ahorro real, el porcentaje es 0.
            promo.PorcentajeAhorro = promo.SumaProductos <= 0 || promo.Ahorro <= 0
                ? 0
                : Math.Round(promo.Ahorro / promo.SumaProductos * 100, 1);

            promo.TieneAhorro = promo.Ahorro > 0;

            promo.EstaActiva = promo.Estado.Equals("ACTIVA", StringComparison.OrdinalIgnoreCase)
                && promo.FechaInicio <= hoy
                && promo.FechaFin >= hoy;
        }

        return base_;
    }
}
