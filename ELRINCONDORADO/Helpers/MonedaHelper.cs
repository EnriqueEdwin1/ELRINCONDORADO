namespace ELRINCONDORADO.Helpers
{
    public static class MonedaHelper
    {
        /// <summary>
        /// Formatea un valor decimal como moneda Bolivianos (Bs)
        /// </summary>
        public static string FormatearBs(decimal valor)
        {
            return $"Bs {valor:N2}";
        }

        /// <summary>
        /// Formatea un valor decimal como moneda Bolivianos sin decimales
        /// </summary>
        public static string FormatearBsSinDecimales(decimal valor)
        {
            return $"Bs {valor:N0}";
        }

        /// <summary>
        /// Formatea un número decimal tal como se ingresó, sin decimales forzados
        /// </summary>
        public static string FormatearNumero(decimal valor)
        {
            return valor.ToString("0.############", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
