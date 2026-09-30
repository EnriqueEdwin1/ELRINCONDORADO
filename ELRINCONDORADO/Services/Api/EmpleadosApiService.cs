using System.Text;
using System.Text.Json;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Services.Api;

public class EmpleadosApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<EmpleadosApiService> _logger;

    public EmpleadosApiService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<EmpleadosApiService> logger)
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

    public async Task<IEnumerable<EmpleadoDto>?> GetAllAsync(int? rol = null, string? estado = null)
    {
        try
        {
            var client = GetClient();
            var queryString = new List<string>();

            if (rol.HasValue) queryString.Add($"rol={rol.Value}");
            if (!string.IsNullOrWhiteSpace(estado)) queryString.Add($"estado={estado}");

            var url = "/api/empleados";
            if (queryString.Any()) url += "?" + string.Join("&", queryString);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<EmpleadoDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empleados");
            return null;
        }
    }

    public async Task<EmpleadoDto?> GetByIdAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync($"/api/empleados/{id}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<EmpleadoDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener empleado {id}");
            return null;
        }
    }

    public async Task<(bool success, string message)> CreateAsync(EmpleadoDto empleado)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(empleado);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/empleados", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Empleado creado exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear empleado");
            return (false, "Error al crear empleado.");
        }
    }

    public async Task<(bool success, string message)> EditAsync(int id, EmpleadoDto empleado)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(empleado);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/empleados/{id}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Empleado actualizado exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al actualizar empleado {id}");
            return (false, "Error al actualizar empleado.");
        }
    }

    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.DeleteAsync($"/api/empleados/{id}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Empleado eliminado exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar empleado {id}");
            return (false, "Error al eliminar empleado.");
        }
    }
}
