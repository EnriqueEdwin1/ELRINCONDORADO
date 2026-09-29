namespace ELRINCONDORADO.API.Models.DTOs;

// Destino de un insumo: COCINA, MESAS, BEBIDAS...
public class DestinoInsumoDto
{
    public int IdDestino { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
