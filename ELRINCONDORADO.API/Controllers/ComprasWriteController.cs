using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ELRINCONDORADO.API.Controllers;

[Authorize]
[ApiController]
[Route("api/compras")]
public class ComprasWriteController : ControllerBase
{
    private readonly AppDbContext _db;

    public ComprasWriteController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CompraConDetallesDto compra)
    {
        var entity = new Compra
        {
            IdProveedor = compra.IdProveedor,
            IdEmpleado = compra.IdEmpleado,
            FechaCompra = compra.FechaCompra,
            Subtotal = compra.Subtotal,
            Descuento = compra.Descuento,
            Total = compra.Total,
            Estado = compra.Estado
        };

        _db.Compras.Add(entity);
        await _db.SaveChangesAsync();

        // Agregar los detalles si vienen y actualizar el stock de insumos
        if (compra.Detalles != null && compra.Detalles.Any())
        {
            foreach (var detalle in compra.Detalles)
            {
                var detalleEntity = new DetalleCompra
                {
                    IdCompra = entity.IdCompra,
                    IdInsumo = detalle.IdInsumo,
                    Cantidad = detalle.Cantidad,
                    CostoUnitario = detalle.CostoUnitario,
                    Subtotal = detalle.Subtotal
                };
                _db.DetalleCompras.Add(detalleEntity);

                // Incrementar el stock del insumo
                var insumo = await _db.Insumos.FindAsync(detalle.IdInsumo);
                if (insumo != null)
                {
                    insumo.StockActual += detalle.Cantidad;
                    _db.Insumos.Update(insumo);
                }
            }
            await _db.SaveChangesAsync();
        }

        return Ok(new { mensaje = "Compra creada exitosamente.", id = entity.IdCompra });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] CompraDto compra)
    {
        if (id != compra.IdCompra)
            return BadRequest(new { mensaje = "El ID no coincide." });

        var entity = await _db.Compras.FindAsync(id);
        if (entity == null)
            return NotFound(new { mensaje = "Compra no encontrada." });

        entity.IdProveedor = compra.IdProveedor;
        entity.IdEmpleado = compra.IdEmpleado;
        entity.FechaCompra = compra.FechaCompra;
        entity.Subtotal = compra.Subtotal;
        entity.Descuento = compra.Descuento;
        entity.Total = compra.Total;
        entity.Estado = compra.Estado;

        _db.Compras.Update(entity);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Compra actualizada exitosamente." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var compra = await _db.Compras.FindAsync(id);
        if (compra == null)
            return NotFound(new { mensaje = "Compra no encontrada." });

        // Primero eliminar los detalles de compra
        var detalles = await _db.DetalleCompras
            .Where(d => d.IdCompra == id)
            .ToListAsync();

        if (detalles.Any())
        {
            _db.DetalleCompras.RemoveRange(detalles);
            await _db.SaveChangesAsync();
        }

        // Luego eliminar la compra
        _db.Compras.Remove(compra);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Compra eliminada exitosamente." });
    }

    [HttpPost("{id:int}/anular")]
    public async Task<IActionResult> Anular(int id)
    {
        var compra = await _db.Compras
            .Include(c => c.DetalleCompras)
            .FirstOrDefaultAsync(c => c.IdCompra == id);

        if (compra == null)
            return NotFound(new { mensaje = "Compra no encontrada." });

        if (compra.Estado == "ANULADO")
            return BadRequest(new { mensaje = "La compra ya está anulada." });

        // Cambiar estado a ANULADO
        compra.Estado = "ANULADO";
        _db.Compras.Update(compra);

        // Descontar el stock de insumos y crear movimientos de inventario
        var empleadoIdStr = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(empleadoIdStr, out var empleadoId))
            empleadoId = 1; // Valor por defecto si no se puede obtener

        foreach (var detalle in compra.DetalleCompras)
        {
            // Descontar el stock del insumo
            var insumo = await _db.Insumos.FindAsync(detalle.IdInsumo);
            if (insumo != null)
            {
                insumo.StockActual -= detalle.Cantidad;
                _db.Insumos.Update(insumo);
            }

            // Crear movimiento de inventario de salida
            var movimiento = new MovimientoInventario
            {
                TipoMovimiento = "SALIDA",
                Cantidad = detalle.Cantidad,
                Fecha = DateTime.Now,
                Motivo = $"ANULACIÓN DE COMPRA #{compra.IdCompra}",
                IdInsumo = detalle.IdInsumo,
                IdEmpleado = empleadoId
            };
            _db.MovimientosInventario.Add(movimiento);
        }

        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Compra anulada exitosamente." });
    }
}
