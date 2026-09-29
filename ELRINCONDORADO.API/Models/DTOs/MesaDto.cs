namespace ELRINCONDORADO.API.Models.DTOs;

public class MesaDto
{
    public int IdMesa { get; set; }
    public int Numero { get; set; }
    public int Capacidad { get; set; }

    // Valores observados en Supabase: DISPONIBLE.
    // El POS del Cajero solo ofrece mesas en este estado.
    public string Estado { get; set; } = string.Empty;
}
