namespace ELRINCONDORADO.API.Data.Entities;

// Mapeada contra la tabla existente 'movimientos_inventario' de Supabase.
// Es el histórico de entradas y salidas de cada insumo.
//
// 'fecha' es 'timestamp without time zone'; la conversion a UTC la resuelve el
// switch global 'Npgsql.EnableLegacyTimestampBehavior' que se activa en Program.cs,
// igual que en el proyecto MVC.
public class MovimientoInventario
{
    public int IdMovimiento { get; set; }
    public int IdInsumo { get; set; }
    public int IdEmpleado { get; set; }

    // ENTRADA, VENTA, AJUSTE, INGRESO
    public string TipoMovimiento { get; set; } = string.Empty;

    // numeric(18,8) en Supabase
    public decimal Cantidad { get; set; }

    public DateTime Fecha { get; set; }
    public string? Motivo { get; set; }

    // NULL mientras el movimiento pertenece al turno abierto. Cuando el cierre de
    // caja se registra queda apuntando al cierre correspondiente. En Supabase esta
    // columna NO tiene clave foránea, por eso no se mapea como navegación.
    public int? IdCierre { get; set; }

    public Insumo? Insumo { get; set; }
    public Empleado? Empleado { get; set; }
}
