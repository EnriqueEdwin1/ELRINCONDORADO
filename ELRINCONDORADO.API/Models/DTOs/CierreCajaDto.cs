namespace ELRINCONDORADO.API.Models.DTOs;

// Un corte de caja.
//
// IMPORTANTE sobre 'totalVentas': lo que trae aquí es el valor que el MVC
// guardó al cerrar, y ese cálculo suma los pedidos del DÍA CALENDARIO, no los
// del turno. No es lo mismo. Para el total real del turno hay que mirar
// 'vendidoTurno' en /api/cierres-caja/resumen, o sumar 'ventasIncluidas' del
// detalle, no este campo.
public class CierreCajaDto
{
    public int IdCierre { get; set; }

    // UTC. OJO: el resto de fechas del sistema (pedidos, compras) son hora local.
    public DateTime Fecha { get; set; }

    public int IdEmpleado { get; set; }
    public string? EmpleadoNombre { get; set; }

    // Lo que guardó el MVC: total del día calendario al momento del cierre.
    public decimal TotalVentas { get; set; }

    // ===== Derivados =====

    // Movimientos de inventario que quedaron asignados a este corte.
    // Se cuenta por 'id_cierre', que en Supabase tampoco tiene clave foránea.
    public int MovimientosCerrados { get; set; }

    // true si IdEmpleado no corresponde a un empleado real. El MVC usa 0 como
    // "no sé", y sin FK nada lo impide.
    public bool EmpleadoDesconocido { get; set; }
}

// Detalle de un corte: qué movimientos de inventario y qué ventas engloba.
public class CierreCajaDetalleDto : CierreCajaDto
{
    // Movimientos de inventario asignados a este corte, agrupados por tipo.
    public List<CierreCajaMovimientoResumen> Movimientos { get; set; } = new();

    // Inicio del intervalo que este corte engloba: el cierre anterior. Si no
    // hay cierre previo, null, porque el turno venía abierto desde antes de que
    // existiera el primer registro.
    public DateTime? TurnoDesde { get; set; }

    // Pedidos no CANCELADOS creados entre 'turnoDesde' y este corte, ambos
    // incluidos. Es el turno real, no el día calendario: por eso NO coincide con
    // 'totalVentas'. Los pedidos posteriores al corte pertenecen al turno
    // siguiente y no aparecen aquí.
    public List<CierreCajaVentaDto> VentasIncluidas { get; set; } = new();
}

// Agrupa los movimientos de un corte por tipo de movimiento.
public class CierreCajaMovimientoResumen
{
    public string TipoMovimiento { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Unidades { get; set; }
}

// Una venta incluida en el total del corte.
public class CierreCajaVentaDto
{
    public int IdPedido { get; set; }
    public string? NombrePedido { get; set; }
    public string? NumeroPedido { get; set; }
    public string TipoPedido { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
}

// DTO para crear un cierre de caja
public class CierreCajaCreateDto
{
    public int IdEmpleado { get; set; }
}
