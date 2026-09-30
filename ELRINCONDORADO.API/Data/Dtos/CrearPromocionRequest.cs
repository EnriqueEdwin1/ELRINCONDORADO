namespace ELRINCONDORADO.API.Data.Dtos;

public class CrearPromocionRequest
{
    public int? IdPromocion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Tipo { get; set; } = "PAQUETE";
    public decimal Valor { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }
    public string Estado { get; set; } = "ACTIVA";
    public string? ImagenUrl { get; set; }
    public string? DisplayUrl { get; set; }
    public List<DetallePromocionRequest>? DetallePromociones { get; set; }
}

public class DetallePromocionRequest
{
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
}
