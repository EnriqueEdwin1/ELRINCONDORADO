using System.Text.Json;
using System.Text;

namespace ELRINCONDORADO.Services.Api
{
    public class PedidosApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PedidosApiService> _logger;

        public PedidosApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PedidosApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient("ElRinconDoradoAPI");
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public async Task<PedidoPaginadoDto?> GetAllAsync(
            string? estado = null,
            string? estadoPago = null,
            string? tipo = null,
            int? idEmpleado = null,
            int? idMesa = null,
            int? idPromocion = null,
            int? idCliente = null,
            DateOnly? desde = null,
            DateOnly? hasta = null,
            string? buscar = null,
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                var client = GetClient();
                var queryString = new List<string>();
                
                if (!string.IsNullOrWhiteSpace(estado)) queryString.Add($"estado={estado}");
                if (!string.IsNullOrWhiteSpace(estadoPago)) queryString.Add($"estadoPago={estadoPago}");
                if (!string.IsNullOrWhiteSpace(tipo)) queryString.Add($"tipo={tipo}");
                if (idEmpleado.HasValue) queryString.Add($"idEmpleado={idEmpleado.Value}");
                if (idMesa.HasValue) queryString.Add($"idMesa={idMesa.Value}");
                if (idPromocion.HasValue) queryString.Add($"idPromocion={idPromocion.Value}");
                if (idCliente.HasValue) queryString.Add($"idCliente={idCliente.Value}");
                if (desde.HasValue) queryString.Add($"desde={desde.Value:yyyy-MM-dd}");
                if (hasta.HasValue) queryString.Add($"hasta={hasta.Value:yyyy-MM-dd}");
                if (!string.IsNullOrWhiteSpace(buscar)) queryString.Add($"buscar={Uri.EscapeDataString(buscar)}");
                queryString.Add($"page={page}");
                queryString.Add($"pageSize={pageSize}");

                var url = "/api/pedidos?" + string.Join("&", queryString);

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PedidoPaginadoDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pedidos");
                return null;
            }
        }

        public async Task<PedidoDto?> GetByIdAsync(int id)
        {
            try
            {
                var client = GetClient();
                var response = await client.GetAsync($"/api/pedidos/{id}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PedidoDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener pedido {id}");
                return null;
            }
        }
    }

    public class PedidoPaginadoDto
    {
        public List<PedidoDto> Pedidos { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
    }

    public class PedidoDto
    {
        public int IdPedido { get; set; }
        public string TipoPedido { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public string? NombrePedido { get; set; }
        public string? NumeroPedido { get; set; }
        public string? Observaciones { get; set; }
        public int? IdMesa { get; set; }
        public int? NumeroMesa { get; set; }
        public int IdEmpleado { get; set; }
        public string? NombreCajero { get; set; }
        public int? IdPromocion { get; set; }
        public string? NombrePromocion { get; set; }
        public int? IdCliente { get; set; }
        public string? NitCliente { get; set; }
        public string? RazonSocialCliente { get; set; }
        public List<DetallePedidoDto> Detalles { get; set; } = new();
    }

    public class DetallePedidoDto
    {
        public int IdDetallePedido { get; set; }
        public int IdPedido { get; set; }
        public int IdProducto { get; set; }
        public string? NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string? Observacion { get; set; }
        public decimal PrecioActual { get; set; }
        public bool PrecioDifiere { get; set; }
    }
}
