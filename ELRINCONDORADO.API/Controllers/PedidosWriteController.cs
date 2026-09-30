using System.Security.Claims;
using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Data.Entities;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

/// Escritura del módulo Pedidos y ventas: el POS del Cajero crea pedidos aquí y la
/// cocina/mesero actualizan el estado. Los getters viven en PedidosController.
[Authorize]
[ApiController]
[Route("api/pedidos")]
public class PedidosWriteController : ControllerBase
{
    private readonly AppDbContext _db;

    public PedidosWriteController(AppDbContext db)
    {
        _db = db;
    }

    /// Crea un pedido (y su detalle) a partir del carrito del POS.
    ///
    /// La transacción cubre la cabecera, las líneas y el alta del cliente por NIT:
    /// si algo falla a mitad de camino, no queda ni la mitad del pedido guardada.
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearPedidoRequest request)
    {
        if (request == null || request.Items == null || request.Items.Count == 0)
            return BadRequest(new { ok = false, mensaje = "El pedido no tiene productos." });

        // El IdEmpleado viaja en el token JWT, no en el body: no se puede falsear.
        var idEmpleadoTexto = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idEmpleadoTexto, out var idEmpleado))
            return Unauthorized(new { ok = false, mensaje = "La sesión no tiene un empleado válido." });

        var tipoPedido = (request.TipoPedido ?? "PARA_LLEVAR").Trim().ToUpperInvariant();
        if (tipoPedido != "PARA_LLEVAR" && tipoPedido != "MESA")
            return BadRequest(new { ok = false, mensaje = "El tipo de pedido no es válido." });

        var estadoPago = (request.MetodoPago ?? "PENDIENTE").Trim().ToUpperInvariant();

        await using var transaccion = await _db.Database.BeginTransactionAsync();

        // ===== Cliente (upsert por NIT) =====
        int? idCliente = null;
        var nit = (request.Nit ?? "").Trim();
        if (nit.Length > 0)
        {
            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Nit == nit);
            if (cliente == null)
            {
                cliente = new Cliente
                {
                    Nit = nit,
                    RazonSocial = string.IsNullOrWhiteSpace(request.RazonSocial)
                        ? "SIN NOMBRE"
                        : request.RazonSocial.Trim()
                };
                _db.Clientes.Add(cliente);
                await _db.SaveChangesAsync();
            }
            idCliente = cliente.IdCliente;
        }

        var pedido = new Pedido
        {
            IdEmpleado = idEmpleado,
            IdCliente = idCliente,
            IdMesa = tipoPedido == "MESA" ? request.IdMesa : null,
            TipoPedido = tipoPedido,
            Estado = "PENDIENTE",
            EstadoPago = estadoPago,
            FechaCreacion = DateTime.Now,
            NombrePedido = string.IsNullOrWhiteSpace(request.NombrePedido)
                ? null
                : request.NombrePedido.Trim(),
            // El número de factura lo asigna el cierre de caja, no el POS.
            NumeroPedido = null,
            Observaciones = request.Observaciones
        };

        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync(); // para tener pedido.IdPedido en las líneas

        // ===== Líneas del pedido =====
        var promosUsadas = new HashSet<int>();
        decimal subtotal = 0m;
        decimal descuento = 0m;

        foreach (var item in request.Items)
        {
            if (item == null || item.Cantidad <= 0)
                return BadRequest(new { ok = false, mensaje = "La cantidad de un producto no es válida." });

            if (item.IdPromocion > 0)
            {
                var promo = await _db.Promociones
                    .Include(p => p.DetallePromociones!)
                        .ThenInclude(dp => dp.Producto)
                    .FirstOrDefaultAsync(p => p.IdPromocion == item.IdPromocion);

                if (promo == null)
                    return BadRequest(new { ok = false, mensaje = "La promoción no existe." });

                promosUsadas.Add(promo.IdPromocion);

                // El paquete se cobra a promo.Valor, pero las líneas se guardan a
                // precio completo de cada producto y el ahorro va como descuento.
                decimal valorComponentes = 0m;
                foreach (var dp in promo.DetallePromociones ?? new List<DetallePromocion>())
                {
                    if (dp.Producto == null) continue;

                    var cantidad = dp.Cantidad * item.Cantidad;
                    var precioUnitario = dp.Producto.Precio;
                    valorComponentes += dp.Cantidad * dp.Producto.Precio;

                    _db.DetallePedidos.Add(new DetallePedido
                    {
                        IdPedido = pedido.IdPedido,
                        IdProducto = dp.Producto.IdProducto,
                        Cantidad = cantidad,
                        PrecioUnitario = precioUnitario,
                        Subtotal = Math.Round(cantidad * precioUnitario, 2),
                        Observacion = item.Observacion
                    });
                    subtotal += cantidad * precioUnitario;
                }

                // El descuento nunca puede quedar en negativo.
                descuento += Math.Max(0m, valorComponentes - promo.Valor) * item.Cantidad;
            }
            else
            {
                var producto = await _db.Productos.FirstOrDefaultAsync(p => p.IdProducto == item.IdProducto);
                if (producto == null)
                    return BadRequest(new { ok = false, mensaje = "El producto no existe." });

                var cantidad = item.Cantidad;
                var precioUnitario = producto.Precio;

                _db.DetallePedidos.Add(new DetallePedido
                {
                    IdPedido = pedido.IdPedido,
                    IdProducto = producto.IdProducto,
                    Cantidad = cantidad,
                    PrecioUnitario = precioUnitario,
                    Subtotal = Math.Round(cantidad * precioUnitario, 2),
                    Observacion = item.Observacion
                });
                subtotal += cantidad * precioUnitario;
            }
        }

        if (subtotal <= 0m)
            return BadRequest(new { ok = false, mensaje = "El pedido no tiene líneas válidas." });

        pedido.Subtotal = Math.Round(subtotal, 2);
        pedido.Descuento = Math.Round(descuento, 2);
        pedido.Total = Math.Round(pedido.Subtotal - pedido.Descuento, 2);

        // La cabecera solo guarda la promoción cuando el pedido usó UNA sola.
        pedido.IdPromocion = promosUsadas.Count == 1 ? promosUsadas.First() : null;

        await _db.SaveChangesAsync();
        await transaccion.CommitAsync();

        return Ok(new { ok = true, idPedido = pedido.IdPedido });
    }

    /// Actualiza el estado de un pedido (PENDIENTE, EN PREPARACION, LISTO,
    /// ENTREGADO, CANCELADO). Lo usa la cocina para preparar/entregar y el mesero
    /// para marcar un pedido LISTO como ENTREGADO.
    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Estado))
            return BadRequest(new { ok = false, mensaje = "Debe indicar el estado." });

        var estado = request.Estado.Trim().ToUpperInvariant();
        var estadosValidos = new[] { "PENDIENTE", "EN PREPARACION", "LISTO", "ENTREGADO", "CANCELADO" };
        if (!estadosValidos.Contains(estado))
            return BadRequest(new { ok = false, mensaje = $"El estado '{request.Estado}' no es válido." });

        var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.IdPedido == id);
        if (pedido == null)
            return NotFound(new { ok = false, mensaje = "El pedido no existe." });

        pedido.Estado = estado;
        await _db.SaveChangesAsync();

        return Ok(new { ok = true });
    }

    /// Marca un pedido como ENTREGADO (solo mesero)
    // [HttpPost("{id:int}/entregar")]
    // [Authorize]
    // [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // public async Task<ActionResult<MensajeResponse>> Entregar(int id)
    // {
    //     var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.IdPedido == id);
    //     if (pedido == null)
    //         return NotFound(new MensajeResponse { mensaje = "El pedido no existe." });

    //     if (pedido.Estado != "LISTO")
    //         return BadRequest(new MensajeResponse { mensaje = "Solo se pueden entregar pedidos que están en estado LISTO." });

    //     pedido.Estado = "ENTREGADO";
    //     await _db.SaveChangesAsync();

    //     return Ok(new MensajeResponse { mensaje = "Pedido entregado exitosamente." });
    // }
}