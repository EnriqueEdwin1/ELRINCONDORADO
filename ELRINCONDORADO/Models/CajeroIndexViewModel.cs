namespace ELRINCONDORADO.Models
{
    public class CajeroIndexViewModel
    {
        public List<Categoria> Categorias { get; set; } = new();
        public List<Promocion> Promociones { get; set; } = new();
        public List<Mesa> Mesas { get; set; } = new();
    }
}