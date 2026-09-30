namespace ELRINCONDORADO.Models.ApiDtos;

public class RolDto
{
    public int IdRol { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CantidadEmpleados { get; set; }
}
