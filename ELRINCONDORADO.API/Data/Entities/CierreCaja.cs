namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'cierres_caja' de Supabase.
// Cada fila es un corte de turno: el momento en que un cajero cierra la caja y
// el total de ventas que había hasta ese instante.
//
// OJO CON TRES COSAS, todas verificadas contra los datos reales:
//
// 1. 'id_empleado' NO tiene clave foránea en Supabase. El MVC lo escribe con
//    ObtenerIdEmpleado() ?? 0, así que un cierre puede quedar con id_empleado = 0
//    si la sesión no traía el empleado. No hay integridad que lo impida.
//
// 2. La hora de esta tabla y la de pedidos NO están en la misma zona. El MVC
//    guarda aquí 'DateTime.UtcNow' (UTC) y en pedidos.fecha_creacion guarda hora
//    local. La sesión de Supabase está en UTC, así que comparar ambas columnas
//    directamente desplaza los pedidos unas horas. Ver Controllers/CierresCajaController.cs.
//
// 3. 'total_ventas' lo calcula el MVC con ConstruirCierreCaja(), que suma los
//    pedidos de HOY (DateTime.Today) cuyo estado no es CANCELADO. No es el total
//    del turno: es el total del día calendario. En los dos cierres que hay hoy
//    da 0.00 porque ambos se hicieron antes del primer pedido de su día.
public class CierreCaja
{
    public int IdCierre { get; set; }

    // 'timestamp without time zone'. El MVC la escribe en UTC (DateTime.UtcNow),
    // a diferencia del resto de fechas del sistema, que son locales.
    public DateTime Fecha { get; set; }

    // Sin FK en Supabase. Puede valer 0 si la sesión no traía empleado.
    public int IdEmpleado { get; set; }

    // numeric(10,2). Total de ventas del DÍA CALENDARIO, no del turno.
    public decimal TotalVentas { get; set; }

    public Empleado? Empleado { get; set; }

    // No se mapea como navegación: en Supabase 'movimientos_inventario.id_cierre'
    // no tiene clave foránea, igual que el resto de referencias de esa tabla.
    // Ver Data\Entities\MovimientoInventario.cs.
}
