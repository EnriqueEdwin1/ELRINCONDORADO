using System.Text;
using System.Text.Json;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Services.Api;

public class ComprasApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ComprasApiService> _logger;

    public ComprasApiService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ComprasApiService> logger)
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

    public async Task<CompraPaginadoDto?> GetAllAsync(
        string? estado = null, int? idProveedor = null, int? idEmpleado = null,
        DateOnly? desde = null, DateOnly? hasta = null, string? buscar = null,
        int page = 1, int pageSize = 50)
    {
        try
        {
            var client = GetClient();
            var queryString = new List<string>();

            if (!string.IsNullOrWhiteSpace(estado)) queryString.Add($"estado={estado}");
            if (idProveedor.HasValue) queryString.Add($"idProveedor={idProveedor.Value}");
            if (idEmpleado.HasValue) queryString.Add($"idEmpleado={idEmpleado.Value}");
            if (desde.HasValue) queryString.Add($"desde={desde.Value:yyyy-MM-dd}");
            if (hasta.HasValue) queryString.Add($"hasta={hasta.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(buscar)) queryString.Add($"buscar={Uri.EscapeDataString(buscar)}");
            queryString.Add($"page={page}");
            queryString.Add($"pageSize={pageSize}");

            var url = "/api/compras?" + string.Join("&", queryString);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CompraPaginadoDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compras");
            return null;
        }
    }

    public async Task<CompraDetalleDto?> GetByIdAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync($"/api/compras/{id}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CompraDetalleDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener compra {id}");
            return null;
        }
    }

    public async Task<CompraResumenDto?> GetResumenAsync(
        DateOnly? desde = null, DateOnly? hasta = null, int? idProveedor = null)
    {
        try
        {
            var client = GetClient();
            var queryString = new List<string>();

            if (desde.HasValue) queryString.Add($"desde={desde.Value:yyyy-MM-dd}");
            if (hasta.HasValue) queryString.Add($"hasta={hasta.Value:yyyy-MM-dd}");
            if (idProveedor.HasValue) queryString.Add($"idProveedor={idProveedor.Value}");

            var url = "/api/compras/resumen";
            if (queryString.Any()) url += "?" + string.Join("&", queryString);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CompraResumenDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen de compras");
            return null;
        }
    }

    public async Task<(bool success, string message)> CreateAsync(CompraDto compra)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(compra);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/compras", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Compra creada exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear compra");
            return (false, "Error al crear compra.");
        }
    }

    public async Task<(bool success, string message)> CreateConDetallesAsync(CompraConDetallesDto compra)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(compra);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/compras", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Compra creada exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear compra con detalles: {Error}", ex.Message);
            return (false, "Error al crear compra.");
        }
    }

    public async Task<(bool success, string message)> EditAsync(int id, CompraDto compra)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(compra);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/compras/{id}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Compra actualizada exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al actualizar compra {id}");
            return (false, "Error al actualizar compra.");
        }
    }

    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.DeleteAsync($"/api/compras/{id}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Compra eliminada exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar compra {id}");
            return (false, "Error al eliminar compra.");
        }
    }

    public async Task<(bool success, string message)> AnularAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.PostAsync($"/api/compras/{id}/anular", null);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Compra anulada exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al anular compra {id}");
            return (false, "Error al anular compra.");
        }
    }
}
