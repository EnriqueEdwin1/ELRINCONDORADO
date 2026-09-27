namespace ELRINCONDORADO.Models
{
    /// <summary>
    /// Formulario de alta/edición de una promoción. La promoción vive en la tabla
    /// promociones y sus productos en detalle_promociones; no tiene categoría.
    /// </summary>
    public class PromocionFormViewModel
    {
        public int IdPromocion { get; set; }

        public string Nombre { get; set; } = "";

        public string? Descripcion { get; set; }

        /// <summary>Precio final de la promoción (columna valor de promociones).</summary>
        public decimal Precio { get; set; }

        /// <summary>ACTIVA o INACTIVA. Sólo las activas aparecen en el cajero.</summary>
        public string Estado { get; set; } = "ACTIVA";

        /// <summary>Imagen ya guardada (solo lectura, se muestra en el formulario de edición).</summary>
        public string? ImagenActual { get; set; }

        /// <summary>Url de ImgBB para borrar la imagen cuando se reemplaza o se quita.</summary>
        public string? ImagenActualDeleteUrl { get; set; }

        public List<ProductoPromocionItem> Incluidos { get; set; } = new List<ProductoPromocionItem>();

        public bool EsEdicion => IdPromocion > 0;

        public decimal SumaProductos => Incluidos.Sum(i => i.Precio * i.Cantidad);

        public decimal Ahorro => SumaProductos - Precio;

        public decimal PorcentajeAhorro
        {
            get
            {
                var suma = SumaProductos;
                if (suma <= 0 || Ahorro <= 0) return 0;
                return Math.Round(Ahorro / suma * 100, 1);
            }
        }
    }
}
