using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Services.Api;

public class PromocionesApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PromocionesApiService> _logger;

    public PromocionesApiService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PromocionesApiService> logger)
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
                new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // El listado ya trae 'Incluidos', así que alcanza con el GET sin filtros.
    public async Task<IEnumerable<PromocionDto>?> GetAllAsync()
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync("/api/promociones");
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<IEnumerable<PromocionDto>>(
                await response.Content.ReadAsStringAsync(), Json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener promociones");
            return null;
        }
    }

    public async Task<PromocionDto?> GetByIdAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync($"/api/promociones/{id}");
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PromocionDto>(
                await response.Content.ReadAsStringAsync(), Json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener promoción {id}");
            return null;
        }
    }

    public async Task<(bool success, string message)> CreateAsync(PromocionRequestDto promocion)
    {
        try
        {
            var client = GetClient();
            var json = JsonSerializer.Serialize(promocion);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/promociones", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Promoción creada exitosamente.");

            return (false, ExtraerMensaje(responseContent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear promoción");
            return (false, $"Error al crear promoción: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> EditAsync(int id, PromocionRequestDto promocion)
    {
        try
        {
            var client = GetClient();
            promocion.IdPromocion = id;

            var json = JsonSerializer.Serialize(promocion);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/promociones/{id}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Promoción actualizada exitosamente.");

            return (false, ExtraerMensaje(responseContent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al actualizar promoción {id}");
            return (false, $"Error al actualizar promoción: {ex.Message}");
        }
    }

    // Quitar la imagen sin tocar el resto de la promoción.
    public async Task<(bool success, string message)> QuitarImagenAsync(int id)
    {
        try
        {
            var actual = await GetByIdAsync(id);
            if (actual == null)
                return (false, "Promoción no encontrada.");

            var request = new PromocionRequestDto
            {
                IdPromocion = id,
                Nombre = actual.Nombre,
                Descripcion = actual.Descripcion,
                Tipo = actual.Tipo,
                Valor = actual.Valor,
                FechaInicio = actual.FechaInicio,
                FechaFin = actual.FechaFin,
                HoraInicio = actual.HoraInicio,
                HoraFin = actual.HoraFin,
                Estado = actual.Estado,
                ImagenUrl = null,
                DisplayUrl = null,
                DetallePromociones = actual.Incluidos
                    .Select(i => new DetallePromocionRequestDto
                    {
                        IdProducto = i.IdProducto,
                        Cantidad = i.Cantidad
                    }).ToList()
            };

            return await EditAsync(id, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al quitar la imagen de la promoción {id}");
            return (false, $"Error al quitar la imagen: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.DeleteAsync($"/api/promociones/{id}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Promoción eliminada exitosamente.");

            return (false, ExtraerMensaje(responseContent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar promoción {id}");
            return (false, "Error al eliminar promoción.");
        }
    }

    // La API responde { "mensaje": "..." } en los errores; si no, se devuelve el
    // cuerpo tal cual para no perder el detalle de la validación.
    private static string ExtraerMensaje(string contenido)
    {
        if (string.IsNullOrWhiteSpace(contenido))
            return "La API no devolvió un mensaje de error.";

        try
        {
            using var doc = JsonDocument.Parse(contenido);
            if (doc.RootElement.TryGetProperty("mensaje", out var mensaje) &&
                mensaje.ValueKind == JsonValueKind.String)
            {
                return mensaje.GetString() ?? contenido;
            }
        }
        catch (JsonException)
        {
            // La respuesta no es JSON: se devuelve tal cual.
        }

        return contenido;
    }
}
