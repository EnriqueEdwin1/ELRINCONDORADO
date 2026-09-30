using System.Text.Json;
using System.Text;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Services.Api
{
    public class ProductosApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ProductosApiService> _logger;

        public ProductosApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProductosApiService> logger)
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

        public async Task<IEnumerable<ProductoDto>?> GetAllAsync(int? categoria = null, bool? activo = null, string? buscar = null)
        {
            try
            {
                var client = GetClient();
                var queryString = new List<string>();
                
                if (categoria.HasValue) queryString.Add($"categoria={categoria.Value}");
                if (activo.HasValue) queryString.Add($"activo={activo.Value}");
                if (!string.IsNullOrWhiteSpace(buscar)) queryString.Add($"buscar={Uri.EscapeDataString(buscar)}");

                var url = "/api/productos";
                if (queryString.Any()) url += "?" + string.Join("&", queryString);

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<ProductoDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos");
                return null;
            }
        }

        public async Task<ProductoDetalleDto?> GetByIdAsync(int id)
        {
            try
            {
                var client = GetClient();
                var response = await client.GetAsync($"/api/productos/{id}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProductoDetalleDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener producto {id}");
                return null;
            }
        }

        public async Task<(bool success, string message)> CreateAsync(ProductoDto producto)
        {
            try
            {
                var client = GetClient();
                var json = JsonSerializer.Serialize(producto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/productos", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return (true, "Producto creado exitosamente.");

                return (false, responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                return (false, "Error al crear producto.");
            }
        }

        public async Task<(bool success, string message)> EditAsync(int id, ProductoDto producto)
        {
            try
            {
                var client = GetClient();
                var json = JsonSerializer.Serialize(producto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"/api/productos/{id}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return (true, "Producto actualizado exitosamente.");

                return (false, responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar producto {id}");
                return (false, "Error al actualizar producto.");
            }
        }

        public async Task<(bool success, string message)> QuitarImagenAsync(int id)
        {
            try
            {
                var client = GetClient();
                var response = await client.PutAsync($"/api/productos/{id}/quitar-imagen", null);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return (true, "Imagen del producto eliminada exitosamente.");

                return (false, responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al quitar la imagen del producto {id}");
                return (false, "Error al quitar la imagen.");
            }
        }

        public async Task<(bool success, string message)> DeleteAsync(int id)
        {
            try
            {
                var client = GetClient();
                var response = await client.DeleteAsync($"/api/productos/{id}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return (true, "Producto eliminado exitosamente.");

                return (false, responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar producto {id}");
                return (false, "Error al eliminar producto.");
            }
        }
    }

}
