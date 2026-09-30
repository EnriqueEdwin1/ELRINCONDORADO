using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

[Authorize]
[ApiController]
[Route("api/productos")]
public class ProductosWriteController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductosWriteController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductoDto producto)
    {
        var entity = new Producto
        {
            IdCategoria = producto.IdCategoria,
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            Precio = producto.Precio,
            Activo = producto.Activo,
            ImagenUrl = producto.ImagenUrl,
            DisplayUrl = producto.DisplayUrl
        };

        _db.Productos.Add(entity);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Producto creado exitosamente.", id = entity.IdProducto });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] ProductoDto producto)
    {
        if (id != producto.IdProducto)
            return BadRequest(new { mensaje = "El ID no coincide." });

        var entity = await _db.Productos.FindAsync(id);
        if (entity == null)
            return NotFound(new { mensaje = "Producto no encontrado." });

        entity.IdCategoria = producto.IdCategoria;
        entity.Codigo = producto.Codigo;
        entity.Nombre = producto.Nombre;
        entity.Descripcion = producto.Descripcion;
        entity.Precio = producto.Precio;
        entity.Activo = producto.Activo;
        entity.ImagenUrl = producto.ImagenUrl;
        entity.DisplayUrl = producto.DisplayUrl;

        _db.Productos.Update(entity);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Producto actualizado exitosamente." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto == null)
            return NotFound(new { mensaje = "Producto no encontrado." });

        _db.Productos.Remove(producto);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Producto eliminado exitosamente." });
    }

    [HttpPut("{id:int}/quitar-imagen")]
    public async Task<IActionResult> QuitarImagen(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto == null)
            return NotFound(new { mensaje = "Producto no encontrado." });

        producto.ImagenUrl = null;
        producto.DisplayUrl = null;

        _db.Productos.Update(producto);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Imagen del producto eliminada exitosamente." });
    }
}
