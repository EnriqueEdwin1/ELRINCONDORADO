using System.Text;
using System.Text.Json;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Services.Api;

public class InsumosApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<InsumosApiService> _logger;

    public InsumosApiService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<InsumosApiService> logger)
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

    public async Task<IEnumerable<InsumoDto>?> GetAllAsync(
        bool? activo = null, int? destino = null, bool? bajoMinimo = null, string? buscar = null)
    {
        try
        {
            var client = GetClient();
            var queryString = new List<string>();

            if (activo.HasValue) queryString.Add($"activo={activo.Value}");
            if (destino.HasValue) queryString.Add($"destino={destino.Value}");
            if (bajoMinimo.HasValue) queryString.Add($"bajoMinimo={bajoMinimo.Value}");
            if (!string.IsNullOrWhiteSpace(buscar)) queryString.Add($"buscar={Uri.EscapeDataString(buscar)}");

            var url = "/api/insumos";
            if (queryString.Any()) url += "?" + string.Join("&", queryString);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<InsumoDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener insumos");
            return null;
        }
    }

    public async Task<InsumoDto?> GetByIdAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync($"/api/insumos/{id}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<InsumoDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener insumo {id}");
            return null;
        }
    }

    public async Task<IEnumerable<DestinoInsumoDto>?> GetDestinosAsync()
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync("/api/insumos/destinos");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<DestinoInsumoDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener destinos de insumo");
            return null;
        }
    }

    public async Task<(bool success, string message)> CreateAsync(InsumoDto insumo)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(insumo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/insumos", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Insumo creado exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear insumo");
            return (false, "Error al crear insumo.");
        }
    }

    public async Task<(bool success, string message)> EditAsync(int id, InsumoDto insumo)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(insumo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/insumos/{id}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Insumo actualizado exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al actualizar insumo {id}");
            return (false, "Error al actualizar insumo.");
        }
    }

    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.DeleteAsync($"/api/insumos/{id}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Insumo eliminado exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar insumo {id}");
            return (false, "Error al eliminar insumo.");
        }
    }

    public async Task<(bool success, string message)> CreateDestinoAsync(DestinoInsumoDto destino)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(destino);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/destinos-insumos", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Destino creado exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear destino de insumo");
            return (false, "Error al crear destino de insumo.");
        }
    }

    public async Task<(bool success, string message)> EditDestinoAsync(int id, DestinoInsumoDto destino)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(destino);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/destinos-insumos/{id}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Destino actualizado exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al actualizar destino de insumo {id}");
            return (false, "Error al actualizar destino de insumo.");
        }
    }

    public async Task<(bool success, string message)> DeleteDestinoAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.DeleteAsync($"/api/destinos-insumos/{id}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Destino eliminado exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar destino de insumo {id}");
            return (false, "Error al eliminar destino de insumo.");
        }
    }
}
