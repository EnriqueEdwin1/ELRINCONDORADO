namespace ELRINCONDORADO.Models.ApiDtos;

public class CajaResumenDto
{
    public bool HayCierres { get; set; }
    public int? UltimoCierreId { get; set; }
    public DateTime? UltimoCierreFecha { get; set; }
    public int VentasTurno { get; set; }
    public decimal VendidoTurno { get; set; }
    public int VentasHoy { get; set; }
    public decimal VendidoHoy { get; set; }
    public int MovimientosPendientes { get; set; }
    public List<CajaResumenPago> PorEstadoPago { get; set; } = new();
}

public class CajaResumenPago
{
    public string EstadoPago { get; set; } = string.Empty;
    public int Pedidos { get; set; }
    public decimal Total { get; set; }
}
