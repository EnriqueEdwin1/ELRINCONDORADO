using System.Runtime.InteropServices;
using ELRINCONDORADO.API.Data;
using Microsoft.EntityFrameworkCore;

namespace ELRINCONDORADO.API.Services.SignalR
{
    /// Aviso sonoro de "pedido nuevo pendiente": cada 10 segundos, mientras exista al menos un
    /// pedido en estado PENDIENTE.
    ///
    /// Suena en el equipo donde corre la aplicación (salida de audio de Windows). Se hizo así
    /// porque el aviso desde el navegador no era confiable: Chrome bloquea la reproducción
    /// automática de audio si la pestaña no recibió un clic previo, y el aviso se perdía en
    /// silencio. Aquí no hay pestaña, no hay política de autoplay y no importa si la pantalla
    /// está en segundo plano o cerrada.
    public class AvisoPedidosCocinaService : BackgroundService
    {
        private const int IntervaloSegundos = 10;
        private const string ArchivoAviso = "nuevo.wav";

        // Flags de winmm: reproducir desde memoria, sin esperar y sin buscar otro sonido.
        private const uint SND_ASYNC = 0x0001;
        private const uint SND_NODEFAULT = 0x0002;
        private const uint SND_MEMORY = 0x0004;

        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        private static extern bool PlaySound(byte[] data, IntPtr module, uint flags);

        [DllImport("winmm.dll", EntryPoint = "PlaySoundW", CharSet = CharSet.Unicode)]
        private static extern bool PlaySoundDesdeArchivo(string? name, IntPtr module, uint flags);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IWebHostEnvironment _entorno;
        private readonly PantallaCocinaTracker _pantallasCocina;
        private readonly ILogger<AvisoPedidosCocinaService> _log;

        // El audio vive en un campo para que el buffer siga existiendo mientras winmm lo reproduce.
        private byte[]? _audio;
        private bool? _habiaPantalla;

        public AvisoPedidosCocinaService(
            IServiceScopeFactory scopeFactory,
            IWebHostEnvironment entorno,
            PantallaCocinaTracker pantallasCocina,
            ILogger<AvisoPedidosCocinaService> log)
        {
            _scopeFactory = scopeFactory;
            _entorno = entorno;
            _pantallasCocina = pantallasCocina;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            CargarAudio();

            using var temporizador = new PeriodicTimer(TimeSpan.FromSeconds(IntervaloSegundos));
            _log.LogInformation("Aviso de cocina activo: cada {0}s si hay pedidos PENDIENTE ({1}).",
                IntervaloSegundos, _audio != null ? "audio cargado" : "AUDIO NO ENCONTRADO");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RevisarPedidos();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "No se pudo revisar la cola de cocina para el aviso sonoro.");
                }

                try
                {
                    if (!await temporizador.WaitForNextTickAsync(stoppingToken)) break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            Detener();
        }

        private async Task RevisarPedidos()
        {
            // El aviso solo se emite si hay una pantalla de cocina abierta (no en el cajero,
            // el administrador ni con la app simplemente encendida).
            var hayPantalla = _pantallasCocina.HayPantallaActiva();
            if (_habiaPantalla != hayPantalla)
            {
                _habiaPantalla = hayPantalla;
                _log.LogInformation(hayPantalla
                    ? "Pantalla de cocina activa: el aviso sonoro queda habilitado."
                    : "No hay pantalla de cocina abierta: el aviso sonoro queda en silencio.");
            }
            if (!hayPantalla) return;

            using var ambito = _scopeFactory.CreateScope();
            var db = ambito.ServiceProvider.GetRequiredService<AppDbContext>();

            var pendientes = await db.Pedidos
                .Where(p => p.Estado == "PENDIENTE")
                .Select(p => p.IdPedido)
                .ToListAsync();

            if (pendientes.Count == 0) return;

            _log.LogInformation("Aviso de cocina emitido: {0} pedido(s) pendiente(s) [{1}].",
                pendientes.Count, string.Join(", ", pendientes));
            Reproducir();
        }

        private void CargarAudio()
        {
            var ruta = Path.Combine(_entorno.WebRootPath ?? string.Empty, "audio", "pedidos", ArchivoAviso);
            try
            {
                if (File.Exists(ruta))
                {
                    _audio = File.ReadAllBytes(ruta);
                }
                else
                {
                    _log.LogWarning("No se encontró el audio del aviso en {0}.", ruta);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "No se pudo leer el audio del aviso en {0}.", ruta);
            }
        }

        private void Reproducir()
        {
            if (_audio == null)
            {
                CargarAudio();
                if (_audio == null) return;
            }

            try
            {
                if (!PlaySound(_audio, IntPtr.Zero, SND_MEMORY | SND_ASYNC | SND_NODEFAULT))
                    _log.LogWarning("Windows no reprodujo el audio del aviso (PlaySound devolvió false).");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error al reproducir el aviso sonoro de cocina.");
            }
        }

        private void Detener()
        {
            try
            {
                // PlaySound con nombre nulo detiene el sonido en curso.
                PlaySoundDesdeArchivo(null, IntPtr.Zero, SND_ASYNC);
            }
            catch
            {
                // nada que hacer al apagar
            }
        }
    }
}
