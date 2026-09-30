using System.Text.Json;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Services.Api;

public class CierresCajaApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CierresCajaApiService> _logger;

    public CierresCajaApiService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<CierresCajaApiService> logger)
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

    public async Task<CierreCajaPaginadoDto?> GetAllAsync(
        int? idEmpleado = null, DateOnly? desde = null, DateOnly? hasta = null,
        int page = 1, int pageSize = 50)
    {
        try
        {
            var client = GetClient();
            var queryString = new List<string>();

            if (idEmpleado.HasValue) queryString.Add($"idEmpleado={idEmpleado.Value}");
            if (desde.HasValue) queryString.Add($"desde={desde.Value:yyyy-MM-dd}");
            if (hasta.HasValue) queryString.Add($"hasta={hasta.Value:yyyy-MM-dd}");
            queryString.Add($"page={page}");
            queryString.Add($"pageSize={pageSize}");

            var url = "/api/cierres-caja?" + string.Join("&", queryString);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CierreCajaPaginadoDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cierres de caja");
            return null;
        }
    }

    public async Task<CajaResumenDto?> GetResumenAsync()
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync("/api/cierres-caja/resumen");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CajaResumenDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen de caja");
            return null;
        }
    }

    public async Task<CierreCajaDetalleDto?> GetByIdAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync($"/api/cierres-caja/{id}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CierreCajaDetalleDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener cierre de caja {id}");
            return null;
        }
    }

    public async Task<bool> CrearCierreAsync(int idEmpleado)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(new { idEmpleado });
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/cierres-caja", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cierre de caja");
            return false;
        }
    }
}
