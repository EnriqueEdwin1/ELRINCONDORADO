namespace ELRINCONDORADO.API.Models.DTOs;

// Insumo del inventario. Expone el stock actual porque el panel de administración
// ya lo muestra; el costo unitario también viene, igual que en la pantalla MVC.
public class InsumoDto
{
    public int IdInsumo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal CostoUnitario { get; set; }
    public bool Activo { get; set; }

    public int? IdDestino { get; set; }
    public string? DestinoNombre { get; set; }

    // Misma regla que Administrador/Index del MVC: cuenta como bajo mínimo solo si
    // tiene un mínimo configurado y el stock está por debajo.
    public bool BajoMinimo { get; set; }
}
