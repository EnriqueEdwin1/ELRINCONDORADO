namespace ELRINCONDORADO.API.Models.DTOs;

// Movimiento del historial de inventario.
//
// 'Fecha' se devuelve tal como está en Supabase ('timestamp without time zone'),
// sin convertir a zona horaria: el proyecto MVC guarda y muestra la hora local del
// restaurante, y convertir en la API cambiaría la hora que ve el usuario.
public class MovimientoInventarioDto
{
    public int IdMovimiento { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public DateTime Fecha { get; set; }
    public string? Motivo { get; set; }

    public int IdInsumo { get; set; }
    public string? InsumoNombre { get; set; }

    public int IdEmpleado { get; set; }
    public string? EmpleadoNombre { get; set; }

    // NULL = turno abierto. Con valor = ya pertenece a un cierre de caja.
    public int? IdCierre { get; set; }
}
