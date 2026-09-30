using System.Text;
using System.Text.Json;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Services.Api;

public class ProveedoresApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ProveedoresApiService> _logger;

    public ProveedoresApiService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ProveedoresApiService> logger)
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

    public async Task<IEnumerable<ProveedorDto>?> GetAllAsync(
        string? estado = null, string? buscar = null, string? ordenarPor = null)
    {
        try
        {
            var client = GetClient();
            var queryString = new List<string>();

            if (!string.IsNullOrWhiteSpace(estado)) queryString.Add($"estado={estado}");
            if (!string.IsNullOrWhiteSpace(buscar)) queryString.Add($"buscar={Uri.EscapeDataString(buscar)}");
            if (!string.IsNullOrWhiteSpace(ordenarPor)) queryString.Add($"ordenarPor={ordenarPor}");

            var url = "/api/proveedores";
            if (queryString.Any()) url += "?" + string.Join("&", queryString);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<ProveedorDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedores");
            return null;
        }
    }

    public async Task<ProveedorDto?> GetByIdAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync($"/api/proveedores/{id}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProveedorDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener proveedor {id}");
            return null;
        }
    }

    public async Task<(bool success, string message)> CreateAsync(ProveedorDto proveedor)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(proveedor);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/proveedores", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Proveedor creado exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear proveedor");
            return (false, "Error al crear proveedor.");
        }
    }

    public async Task<(bool success, string message)> EditAsync(int id, ProveedorDto proveedor)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(proveedor);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/proveedores/{id}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Proveedor actualizado exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al actualizar proveedor {id}");
            return (false, "Error al actualizar proveedor.");
        }
    }

    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.DeleteAsync($"/api/proveedores/{id}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Proveedor eliminado exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar proveedor {id}");
            return (false, "Error al eliminar proveedor.");
        }
    }
}
