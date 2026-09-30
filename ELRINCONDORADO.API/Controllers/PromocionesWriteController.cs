using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Dtos;
using ELRINCONDORADO.API.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

[Authorize]
[ApiController]
[Route("api/promociones")]
public class PromocionesWriteController : ControllerBase
{
    private readonly AppDbContext _db;

    public PromocionesWriteController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearPromocionRequest request)
    {
        // Crear la promoción desde el request
        var promocion = new Promocion
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Tipo = request.Tipo,
            Valor = request.Valor,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            HoraInicio = request.HoraInicio,
            HoraFin = request.HoraFin,
            Estado = request.Estado,
            ImagenUrl = request.ImagenUrl,
            DisplayUrl = request.DisplayUrl
        };

        _db.Promociones.Add(promocion);
        await _db.SaveChangesAsync();

        // Guardar los detalles con el ID de promoción asignado
        if (request.DetallePromociones != null && request.DetallePromociones.Count > 0)
        {
            foreach (var detalle in request.DetallePromociones)
            {
                // Validar que el producto existe
                var productoExiste = await _db.Productos.AnyAsync(p => p.IdProducto == detalle.IdProducto);
                if (!productoExiste)
                    return BadRequest(new { mensaje = $"El producto con ID {detalle.IdProducto} no existe." });

                var detallePromocion = new DetallePromocion
                {
                    IdPromocion = promocion.IdPromocion,
                    IdProducto = detalle.IdProducto,
                    Cantidad = detalle.Cantidad
                };
                _db.DetallePromociones.Add(detallePromocion);
            }
            await _db.SaveChangesAsync();
        }

        return Ok(new { mensaje = "Promoción creada exitosamente.", id = promocion.IdPromocion });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] CrearPromocionRequest request)
    {
        var promocion = await _db.Promociones.FindAsync(id);
        if (promocion == null)
            return NotFound(new { mensaje = "Promoción no encontrada." });

        // Actualizar datos básicos
        promocion.Nombre = request.Nombre;
        promocion.Descripcion = request.Descripcion;
        promocion.Tipo = request.Tipo;
        promocion.Valor = request.Valor;
        promocion.FechaInicio = request.FechaInicio;
        promocion.FechaFin = request.FechaFin;
        promocion.HoraInicio = request.HoraInicio;
        promocion.HoraFin = request.HoraFin;
        promocion.Estado = request.Estado;
        promocion.ImagenUrl = request.ImagenUrl;
        promocion.DisplayUrl = request.DisplayUrl;

        // Borrar los detalles anteriores
        var detallesAnteriores = await _db.DetallePromociones
            .Where(d => d.IdPromocion == id)
            .ToListAsync();

        _db.DetallePromociones.RemoveRange(detallesAnteriores);
        await _db.SaveChangesAsync();

        // Guardar los nuevos detalles
        if (request.DetallePromociones != null && request.DetallePromociones.Count > 0)
        {
            foreach (var detalle in request.DetallePromociones)
            {
                // Validar que el producto existe
                var productoExiste = await _db.Productos.AnyAsync(p => p.IdProducto == detalle.IdProducto);
                if (!productoExiste)
                    return BadRequest(new { mensaje = $"El producto con ID {detalle.IdProducto} no existe." });

                var detallePromocion = new DetallePromocion
                {
                    IdPromocion = id,
                    IdProducto = detalle.IdProducto,
                    Cantidad = detalle.Cantidad
                };
                _db.DetallePromociones.Add(detallePromocion);
            }
            await _db.SaveChangesAsync();
        }

        _db.Promociones.Update(promocion);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Promoción actualizada exitosamente." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var promocion = await _db.Promociones.FindAsync(id);
        if (promocion == null)
            return NotFound(new { mensaje = "Promoción no encontrada." });

        _db.Promociones.Remove(promocion);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Promoción eliminada exitosamente." });
    }
}
