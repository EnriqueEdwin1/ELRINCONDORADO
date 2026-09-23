using System.Collections.Generic;

namespace ELRINCONDORADO.Models
{
    public class DestinoInsumo
    {
        public int IdDestino { get; set; }
        public string Nombre { get; set; }

        public ICollection<Insumo>? Insumos { get; set; }
    }
}