using System.Text.Json;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Services.Api;

public class MeseroApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<MeseroApiService> _logger;

    public MeseroApiService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<MeseroApiService> logger)
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

    public async Task<IEnumerable<PedidoDetalleDto>?> GetPedidosEntregarAsync()
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync("/api/pedidos?estado=LISTO&pageSize=200");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var paginado = JsonSerializer.Deserialize<PedidoPaginadoDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (paginado?.Items == null) return null;

            // Obtener los detalles de cada pedido individualmente
            var pedidosConDetalle = new List<PedidoDetalleDto>();
            foreach (var pedido in paginado.Items)
            {
                var detalleResponse = await client.GetAsync($"/api/pedidos/{pedido.IdPedido}");
                detalleResponse.EnsureSuccessStatusCode();

                var detalleContent = await detalleResponse.Content.ReadAsStringAsync();
                var pedidoDetalle = JsonSerializer.Deserialize<PedidoDetalleDto>(detalleContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (pedidoDetalle != null)
                {
                    pedidosConDetalle.Add(pedidoDetalle);
                }
            }

            return pedidosConDetalle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener pedidos para entregar");
            return null;
        }
    }

    public async Task<(bool success, string message)> EntregarAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.PostAsync($"/api/pedidos/{id}/entregar", null);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Pedido entregado exitosamente.");

            return (false, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al entregar pedido {id}");
            return (false, "Error al entregar pedido.");
        }
    }

    public async Task<IEnumerable<PedidoDetalleDto>?> GetHistorialAsync(int idEmpleado)
    {
        try
        {
            var client = GetClient();
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            // Obtener todos los pedidos ENTREGADOS del día sin filtrar por empleado
            var response = await client.GetAsync($"/api/pedidos?estado=ENTREGADO&desde={hoy:yyyy-MM-dd}&hasta={hoy:yyyy-MM-dd}&pageSize=200");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var paginado = JsonSerializer.Deserialize<PedidoPaginadoDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (paginado?.Items == null) return null;

            // Obtener los detalles de cada pedido individualmente
            var pedidosConDetalle = new List<PedidoDetalleDto>();
            foreach (var pedido in paginado.Items)
            {
                var detalleResponse = await client.GetAsync($"/api/pedidos/{pedido.IdPedido}");
                detalleResponse.EnsureSuccessStatusCode();

                var detalleContent = await detalleResponse.Content.ReadAsStringAsync();
                var pedidoDetalle = JsonSerializer.Deserialize<PedidoDetalleDto>(detalleContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (pedidoDetalle != null)
                {
                    pedidosConDetalle.Add(pedidoDetalle);
                }
            }

            return pedidosConDetalle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial de pedidos");
            return null;
        }
    }
}
