using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

// Módulo Proveedores. Solo lectura.
[ApiController]
[Route("api/proveedores")]
public class ProveedoresController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProveedoresController(AppDbContext db)
    {
        _db = db;
    }

    // Filtros opcionales:
    //   ?estado=ACTIVO     solo activos
    //   ?buscar=hiper      busca en nombre, contacto y observaciones
    //   ?ordenarPor=nombre | gastado   (gastado de mayor a menor)
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProveedorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProveedorDto>>> GetAll(
        [FromQuery] string? estado,
        [FromQuery] string? buscar,
        [FromQuery] string? ordenarPor,
        CancellationToken ct)
    {
        var consulta = _db.Proveedores.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var v = estado.Trim().ToUpperInvariant();
            consulta = consulta.Where(p => p.Estado.ToUpper() == v);
        }

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(p =>
                EF.Functions.ILike(p.Nombre, $"%{texto}%") ||
                (p.Telefono != null && EF.Functions.ILike(p.Telefono, $"%{texto}%")) ||
                (p.Email != null && EF.Functions.ILike(p.Email, $"%{texto}%")) ||
                (p.Direccion != null && EF.Functions.ILike(p.Direccion, $"%{texto}%")) ||
                (p.Observaciones != null && EF.Functions.ILike(p.Observaciones, $"%{texto}%")));
        }

        // Los totales de compra se agregan aparte para poder ordenar por ellos en
        // SQL, en vez de traer los proveedores y ordenarlos en memoria.
        var proveedores = await consulta
            .OrderBy(p => p.Nombre)
            .Select(p => new ProveedorDto
            {
                IdProveedor = p.IdProveedor,
                Nombre = p.Nombre,
                Telefono = p.Telefono,
                Direccion = p.Direccion,
                Email = p.Email,
                Estado = p.Estado,
                Observaciones = p.Observaciones,
                TotalCompras = p.Compras!.Count(),
                TotalGastado = p.Compras!.Sum(c => c.Total),
                UltimaCompra = p.Compras!.Max(c => (DateTime?)c.FechaCompra)
            })
            .ToListAsync(ct);

        if (ordenarPor?.Equals("gastado", StringComparison.OrdinalIgnoreCase) == true)
        {
            proveedores = proveedores.OrderByDescending(p => p.TotalGastado).ToList();
        }

        return Ok(proveedores);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProveedorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProveedorDto>> GetById(int id, CancellationToken ct)
    {
        var proveedor = await _db.Proveedores
            .AsNoTracking()
            .Where(p => p.IdProveedor == id)
            .Select(p => new ProveedorDto
            {
                IdProveedor = p.IdProveedor,
                Nombre = p.Nombre,
                Telefono = p.Telefono,
                Direccion = p.Direccion,
                Email = p.Email,
                Estado = p.Estado,
                Observaciones = p.Observaciones,
                TotalCompras = p.Compras!.Count(),
                TotalGastado = p.Compras!.Sum(c => c.Total),
                UltimaCompra = p.Compras!.Max(c => (DateTime?)c.FechaCompra)
            })
            .FirstOrDefaultAsync(ct);

        return proveedor is null ? NotFound() : Ok(proveedor);
    }
}
