using System.Text.Json;
using System.Text;

namespace ELRINCONDORADO.Services.Api
{
    public class ProductosApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ProductosApiService> _logger;

        public ProductosApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProductosApiService> logger)
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

        public async Task<IEnumerable<ProductoDto>?> GetAllAsync(int? categoria = null, bool? activo = null, string? buscar = null)
        {
            try
            {
                var client = GetClient();
                var queryString = new List<string>();
                
                if (categoria.HasValue) queryString.Add($"categoria={categoria.Value}");
                if (activo.HasValue) queryString.Add($"activo={activo.Value}");
                if (!string.IsNullOrWhiteSpace(buscar)) queryString.Add($"buscar={Uri.EscapeDataString(buscar)}");

                var url = "/api/productos";
                if (queryString.Any()) url += "?" + string.Join("&", queryString);

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<ProductoDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos");
                return null;
            }
        }

        public async Task<ProductoDetalleDto?> GetByIdAsync(int id)
        {
            try
            {
                var client = GetClient();
                var response = await client.GetAsync($"/api/productos/{id}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProductoDetalleDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener producto {id}");
                return null;
            }
        }
    }

    public class ProductoDto
    {
        public int IdProducto { get; set; }
        public string? Codigo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public bool Activo { get; set; }
        public string? ImagenUrl { get; set; }
        public string? DisplayUrl { get; set; }
        public int IdCategoria { get; set; }
        public string? CategoriaNombre { get; set; }
        public bool TieneReceta { get; set; }
    }

    public class ProductoDetalleDto : ProductoDto
    {
        public RecetaDto? Receta { get; set; }
    }

    public class RecetaDto
    {
        public int IdReceta { get; set; }
        public int IdProducto { get; set; }
        public string? ProductoNombre { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public List<RecetaDetalleDto> Detalles { get; set; } = new();
    }

    public class RecetaDetalleDto
    {
        public int IdDetalleReceta { get; set; }
        public int IdInsumo { get; set; }
        public string? InsumoNombre { get; set; }
        public decimal Cantidad { get; set; }
        public string? UnidadMedida { get; set; }
    }
}
