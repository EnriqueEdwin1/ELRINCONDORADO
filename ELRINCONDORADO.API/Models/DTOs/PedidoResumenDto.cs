namespace ELRINCONDORADO.API.Models.DTOs;

// Totales de ventas para el panel. No es una tabla: se agrega en cada llamada
// sobre los pedidos que cumplen el rango de fechas.
//
// Ojo con los pedidos CANCELADOS: por defecto se EXCLUYEN, porque sumar una venta
// cancelada infla los ingresos. Si necesitas verlos, pasa ?incluirCancelados=true.
//
// El valor en la base es 'CANCELADO'. No existe 'ANULADO' en este sistema.
public class PedidoResumenDto
{
    public int TotalPedidos { get; set; }
    public decimal TotalSubtotal { get; set; }
    public decimal TotalDescuento { get; set; }
    public decimal TotalVentas { get; set; }

    // Promedio por pedido, sobre los pedidos contados en TotalPedidos.
    public decimal TicketPromedio { get; set; }

    public List<PedidoResumenAgrupado> PorEstadoPago { get; set; } = new();
    public List<PedidoResumenAgrupado> PorTipo { get; set; } = new();
    public List<PedidoResumenDia> PorDia { get; set; } = new();
}

// Un corte de los totales por una dimensión (método de pago o tipo de pedido).
public class PedidoResumenAgrupado
{
    public string Clave { get; set; } = string.Empty;
    public int Pedidos { get; set; }
    public decimal Total { get; set; }
}

// Totales de un día concreto. La fecha viene en formato yyyy-MM-dd.
public class PedidoResumenDia
{
    public DateOnly Fecha { get; set; }
    public int Pedidos { get; set; }
    public decimal Total { get; set; }
}
