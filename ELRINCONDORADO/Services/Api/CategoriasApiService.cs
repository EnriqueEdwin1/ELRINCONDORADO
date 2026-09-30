using System.Text;
using System.Text.Json;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Services.Api;

public class CategoriasApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CategoriasApiService> _logger;

    public CategoriasApiService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<CategoriasApiService> logger)
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

    public async Task<IEnumerable<CategoriaDto>?> GetAllAsync()
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync("/api/categorias");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<CategoriaDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorías");
            return null;
        }
    }

    public async Task<CategoriaDto?> GetByIdAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync($"/api/categorias/{id}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CategoriaDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener categoría {id}");
            return null;
        }
    }

    public async Task<(bool success, string message)> CreateAsync(CategoriaDto categoria)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(categoria);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/categorias", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Categoría creada exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear categoría");
            return (false, "Error al crear categoría.");
        }
    }

    public async Task<(bool success, string message)> EditAsync(int id, CategoriaDto categoria)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(categoria);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/categorias/{id}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Categoría actualizada exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al actualizar categoría {id}");
            return (false, "Error al actualizar categoría.");
        }
    }

    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.DeleteAsync($"/api/categorias/{id}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Categoría eliminada exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar categoría {id}");
            return (false, "Error al eliminar categoría.");
        }
    }
}
