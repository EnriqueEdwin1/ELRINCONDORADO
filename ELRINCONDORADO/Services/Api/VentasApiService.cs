using System.Text.Json;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Services.Api;

public class VentasApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<VentasApiService> _logger;

    public VentasApiService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<VentasApiService> logger)
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
        string? estado = null, string? estadoPago = null, string? tipo = null,
        int? idEmpleado = null, int? idMesa = null, int? idPromocion = null,
        int? idCliente = null, DateOnly? desde = null, DateOnly? hasta = null,
        string? buscar = null, int page = 1, int pageSize = 50)
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
            _logger.LogError(ex, "Error al obtener ventas");
            return null;
        }
    }

    public async Task<PedidoDetalleDto?> GetByIdAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync($"/api/pedidos/{id}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PedidoDetalleDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener venta {id}");
            return null;
        }
    }
}
