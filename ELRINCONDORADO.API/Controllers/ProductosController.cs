using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

// Módulo Productos. Réplica en solo lectura de la pestaña 'productos' de
// Controllers/ProductosController.cs del MVC, sin vistas ni TempData.
// La columna 'delete_url' nunca sale de aquí: ver Data/Entities/Producto.cs.
[ApiController]
[Route("api/productos")]
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductosController(AppDbContext db)
    {
        _db = db;
    }

    // Equivale a Productos/Index. El MVC los ordena por id descendente (más
    // recientes primero) y trae la categoría con Include.
    //
    // Filtros opcionales:
    //   ?categoria=3      solo productos de esa categoría
    //   ?activo=true      solo activos (lo que necesita el POS del cajero)
    //   ?buscar=pollo     busca en nombre, código y descripción
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> GetAll(
        [FromQuery] int? categoria,
        [FromQuery] bool? activo,
        [FromQuery] string? buscar,
        CancellationToken ct)
    {
        var consulta = _db.Productos.AsNoTracking();

        if (categoria.HasValue)
        {
            consulta = consulta.Where(p => p.IdCategoria == categoria.Value);
        }

        if (activo.HasValue)
        {
            consulta = consulta.Where(p => p.Activo == activo.Value);
        }

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            // ILIKE, no Like: el MVC busca sin distinguir mayúsculas.
            consulta = consulta.Where(p =>
                EF.Functions.ILike(p.Nombre, $"%{texto}%") ||
                (p.Codigo != null && EF.Functions.ILike(p.Codigo, $"%{texto}%")) ||
                (p.Descripcion != null && EF.Functions.ILike(p.Descripcion, $"%{texto}%")));
        }

        var productos = await consulta
            .OrderByDescending(p => p.IdProducto)
            .Select(p => new ProductoDto
            {
                IdProducto = p.IdProducto,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                Activo = p.Activo,
                ImagenUrl = p.ImagenUrl,
                DisplayUrl = p.DisplayUrl,
                IdCategoria = p.IdCategoria,
                CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : null,
                // Any() se resuelve en SQL: no se arrastra la receta al listado.
                TieneReceta = _db.Recetas.Any(r => r.IdProducto == p.IdProducto)
            })
            .ToListAsync(ct);

        return Ok(productos);
    }

    // Equivale a Productos/Details/{id}: producto con su receta, la receta con sus
    // insumos y cada insumo con su nombre.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductoDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoDetalleDto>> GetById(int id, CancellationToken ct)
    {
        var producto = await _db.Productos
            .AsNoTracking()
            .Where(p => p.IdProducto == id)
            .Select(p => new ProductoDetalleDto
            {
                IdProducto = p.IdProducto,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                Activo = p.Activo,
                ImagenUrl = p.ImagenUrl,
                DisplayUrl = p.DisplayUrl,
                IdCategoria = p.IdCategoria,
                CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : null,
                TieneReceta = p.Receta != null,
                Receta = p.Receta == null ? null : new RecetaDto
                {
                    IdReceta = p.Receta.IdReceta,
                    IdProducto = p.Receta.IdProducto,
                    ProductoNombre = p.Nombre,
                    Descripcion = p.Receta.Descripcion,
                    Activo = p.Receta.Activo,
                    Detalles = p.Receta.DetalleRecetas!
                        .OrderBy(d => d.Insumo!.Nombre)
                        .Select(d => new RecetaDetalleDto
                        {
                            IdDetalleReceta = d.IdDetalleReceta,
                            IdInsumo = d.IdInsumo,
                            InsumoNombre = d.Insumo != null ? d.Insumo.Nombre : null,
                            Cantidad = d.Cantidad,
                            UnidadMedida = d.UnidadMedida
                        })
                        .ToList()
                }
            })
            .FirstOrDefaultAsync(ct);

        if (producto is null)
        {
            return NotFound();
        }

        return Ok(producto);
    }
}
