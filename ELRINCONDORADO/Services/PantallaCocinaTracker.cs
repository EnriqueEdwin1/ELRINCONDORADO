using System.Collections.Concurrent;

namespace ELRINCONDORADO.Services
{
    /// Registra qué pantallas de cocina están abiertas.
    ///
    /// El aviso sonoro sale del propio equipo (bocinas de Windows), así que sin esto sonaría en
    /// cualquier pantalla que esté usando la app (cajero, administrador, etc.). La pantalla de
    /// cocina envía un latido cada 10 s a /Cocina/Ping; si dejan de llegar más de 30 s, se
    /// considera que ya no hay ninguna pantalla de cocina abierta y el aviso se detiene.
    public class PantallaCocinaTracker
    {
        private static readonly TimeSpan Tolerancia = TimeSpan.FromSeconds(30);

        private readonly ConcurrentDictionary<string, DateTime> _pantallas = new();

        /// Registra el latido de una pantalla de cocina (id de sesión del navegador).
        public void Registrar(string idSesion)
        {
            if (string.IsNullOrEmpty(idSesion)) return;
            _pantallas[idSesion] = DateTime.UtcNow;
        }

        /// ¿Hay alguna pantalla de cocina abierta en este momento?
        public bool HayPantallaActiva()
        {
            var limite = DateTime.UtcNow - Tolerancia;
            var algunaActiva = false;

            foreach (var pantalla in _pantallas)
            {
                if (pantalla.Value >= limite)
                {
                    algunaActiva = true;
                }
                else
                {
                    // Se limpia la que lleva más de 30 s sin latido.
                    _pantallas.TryRemove(pantalla.Key, out _);
                }
            }

            return algunaActiva;
        }
    }
}
