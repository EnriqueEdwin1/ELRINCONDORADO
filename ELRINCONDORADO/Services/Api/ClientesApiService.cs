using System.Text.Json;
using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Services.Api
{
    public class ClientesApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ClientesApiService> _logger;

        public ClientesApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ClientesApiService> logger)
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

        /// Busca un cliente por NIT (GET /api/clientes?nit=). Devuelve null si no se
        /// pudo consultar a la API; la API responde ok=false con mensaje cuando el
        /// NIT no está registrado.
        public async Task<ClienteBusquedaDto?> BuscarAsync(string nit)
        {
            try
            {
                var client = GetClient();
                var url = $"/api/clientes?nit={Uri.EscapeDataString(nit ?? "")}";

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ClienteBusquedaDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar cliente por NIT");
                return null;
            }
        }
    }
}