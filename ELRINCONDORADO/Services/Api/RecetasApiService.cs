using System.Text;
using System.Text.Json;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Services.Api;

public class RecetasApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RecetasApiService> _logger;

    public RecetasApiService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<RecetasApiService> logger)
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

    public async Task<IEnumerable<RecetaDto>?> GetAllAsync()
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync("/api/recetas");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<RecetaDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener recetas");
            return null;
        }
    }

    public async Task<RecetaDto?> GetByIdAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync($"/api/recetas/{id}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RecetaDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener receta {id}");
            return null;
        }
    }

    public async Task<(bool success, string message)> CreateAsync(RecetaDto receta)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(receta);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/recetas", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Receta creada exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear receta");
            return (false, "Error al crear receta.");
        }
    }

    public async Task<(bool success, string message)> EditAsync(int id, RecetaDto receta)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(receta);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/recetas/{id}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Receta actualizada exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al actualizar receta {id}");
            return (false, "Error al actualizar receta.");
        }
    }

    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.DeleteAsync($"/api/recetas/{id}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Receta eliminada exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar receta {id}");
            return (false, "Error al eliminar receta.");
        }
    }
}
