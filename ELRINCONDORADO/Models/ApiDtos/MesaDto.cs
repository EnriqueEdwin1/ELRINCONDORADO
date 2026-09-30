namespace ELRINCONDORADO.Models.ApiDtos;

public class MesaDto
{
    public int IdMesa { get; set; }
    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public string Estado { get; set; } = string.Empty;
}
