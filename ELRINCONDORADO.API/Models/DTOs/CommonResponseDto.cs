namespace ELRINCONDORADO.API.Models.DTOs;

public class MensajeResponse
{
    public string mensaje { get; set; } = string.Empty;
}

public class CierreCajaCreateResponse
{
    public string mensaje { get; set; } = string.Empty;
    public int id { get; set; }
}
