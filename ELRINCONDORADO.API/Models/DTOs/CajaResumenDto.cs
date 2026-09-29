namespace ELRINCONDORADO.API.Models.DTOs;

// Estado actual de la caja.
//
// El turno, a diferencia de 'totalVentas' de un cierre, sí se calcula sobre el
// intervalo real: desde el último cierre hasta ahora. Es lo que usa
// AdministradorController del MVC para el panel.
public class CajaResumenDto
{
    public bool HayCierres { get; set; }

    // Último corte registrado, o null si la caja nunca se ha cerrado.
    public int? UltimoCierreId { get; set; }
    public DateTime? UltimoCierreFecha { get; set; }

    // Turno actual: ventas facturadas posteriores al último cierre.
    public int VentasTurno { get; set; }
    public decimal VendidoTurno { get; set; }

    // Ventas facturadas del día calendario, en hora local.
    public int VentasHoy { get; set; }
    public decimal VendidoHoy { get; set; }

    // Partidas de inventario todavía sin cerrar (id_cierre IS NULL).
    public int MovimientosPendientes { get; set; }

    // Desglose por método de pago del turno.
    public List<CajaResumenPago> PorEstadoPago { get; set; } = new();
}

// Importe de un método de pago dentro del turno.
public class CajaResumenPago
{
    public string EstadoPago { get; set; } = string.Empty;
    public int Pedidos { get; set; }
    public decimal Total { get; set; }
}
