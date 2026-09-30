using ELRINCONDORADO.API.Data;
using ELRINCONDORADO.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Controllers;

/// Búsqueda de clientes por NIT para el POS del Cajero. Es una lectura abierta
/// (igual que el resto de los GET de la API): el NIT es un dato tributario, pero
/// el POS necesita consultarlo sin obligar a que el cajero tenga token.
[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ClientesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ClienteBusquedaDto>> BuscarPorNit([FromQuery] string? nit)
    {
        var nitLimpio = (nit ?? "").Trim();

        if (nitLimpio.Length == 0)
            return Ok(new ClienteBusquedaDto { Ok = false, Mensaje = "Ingrese un NIT para buscar." });

        // NIT "0" es el cliente genérico que el POS usa para facturar sin cliente:
        // existe en la base con razón social "SIN NOMBRE" y no debe editarse.
        if (nitLimpio == "0")
            return Ok(new ClienteBusquedaDto { Ok = true, RazonSocial = "SIN NOMBRE", Bloqueado = true });

        var cliente = await _db.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Nit == nitLimpio);

        if (cliente == null)
            return Ok(new ClienteBusquedaDto { Ok = false, Mensaje = "El NIT no está registrado." });

        return Ok(new ClienteBusquedaDto { Ok = true, RazonSocial = cliente.RazonSocial });
    }
}