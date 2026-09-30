using System.Text;
using System.Text.Json;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Services.Api;

public class MovimientosInventarioApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<MovimientosInventarioApiService> _logger;

    public MovimientosInventarioApiService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<MovimientosInventarioApiService> logger)
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

    public async Task<IEnumerable<MovimientoInventarioDto>?> GetAllAsync(
        int? insumo = null, string? tipo = null, int? empleado = null,
        int? idCierre = null, bool? soloTurnoActual = null, bool? incluirEntradas = null,
        string? buscar = null)
    {
        try
        {
            var client = GetClient();
            var queryString = new List<string>();

            if (insumo.HasValue) queryString.Add($"insumo={insumo.Value}");
            if (!string.IsNullOrWhiteSpace(tipo)) queryString.Add($"tipo={tipo}");
            if (empleado.HasValue) queryString.Add($"empleado={empleado.Value}");
            if (idCierre.HasValue) queryString.Add($"idCierre={idCierre.Value}");
            if (soloTurnoActual.HasValue) queryString.Add($"soloTurnoActual={soloTurnoActual.Value}");
            if (incluirEntradas.HasValue) queryString.Add($"incluirEntradas={incluirEntradas.Value}");
            if (!string.IsNullOrWhiteSpace(buscar)) queryString.Add($"buscar={Uri.EscapeDataString(buscar)}");

            var url = "/api/movimientos-inventario";
            if (queryString.Any()) url += "?" + string.Join("&", queryString);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<MovimientoInventarioDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener movimientos de inventario");
            return null;
        }
    }

    public async Task<MovimientoInventarioDto?> GetByIdAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync($"/api/movimientos-inventario/{id}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MovimientoInventarioDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener movimiento de inventario {id}");
            return null;
        }
    }

    public async Task<(bool success, string message)> CreateAsync(MovimientoInventarioDto movimiento)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(movimiento);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/movimientos-inventario", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Movimiento creado exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear movimiento de inventario");
            return (false, "Error al crear movimiento de inventario.");
        }
    }

    public async Task<(bool success, string message)> EditAsync(int id, MovimientoInventarioDto movimiento)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(movimiento);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/movimientos-inventario/{id}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Movimiento actualizado exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al actualizar movimiento de inventario {id}");
            return (false, "Error al actualizar movimiento de inventario.");
        }
    }

    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.DeleteAsync($"/api/movimientos-inventario/{id}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Movimiento eliminado exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar movimiento de inventario {id}");
            return (false, "Error al eliminar movimiento de inventario.");
        }
    }
}
