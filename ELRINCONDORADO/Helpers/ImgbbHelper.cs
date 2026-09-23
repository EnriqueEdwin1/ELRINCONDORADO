using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ELRINCONDORADO.Helpers
{
    public static class ImgbbHelper
    {
        private const string UploadUrl = "https://api.imgbb.com/1/upload";

        private static readonly HttpClient _http = new();

        public static async Task<ImgbbResult?> SubirImagenAsync(IFormFile imagen, string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || imagen == null || imagen.Length == 0)
                return null;

            using var ms = new MemoryStream();
            await imagen.CopyToAsync(ms);

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(apiKey), "key");

            var contenido = new ByteArrayContent(ms.ToArray());
            var contentType = string.IsNullOrEmpty(imagen.ContentType)
                ? "application/octet-stream"
                : imagen.ContentType;
            contenido.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            form.Add(contenido, "image", imagen.FileName);

            try
            {
                var respuesta = await _http.PostAsync(UploadUrl, form);
                if (!respuesta.IsSuccessStatusCode)
                    return null;

                var json = await respuesta.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<ImgbbResponse>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (resultado?.Data == null || resultado.Success != true)
                    return null;

                return new ImgbbResult(
                    Url: resultado.Data.Url,
                    DisplayUrl: resultado.Data.DisplayUrl,
                    DeleteUrl: resultado.Data.DeleteUrl);
            }
            catch
            {
                return null;
            }
        }

        public static async Task EliminarImagenAsync(string? deleteUrl)
        {
            if (string.IsNullOrEmpty(deleteUrl))
                return;

            try
            {
                await _http.GetAsync(deleteUrl);
            }
            catch
            {
                // Mejor esfuerzo: si falla la eliminación remota no se bloquea el flujo.
            }
        }

        private class ImgbbResponse
        {
            [JsonPropertyName("data")]
            public ImgbbData? Data { get; set; }

            [JsonPropertyName("success")]
            public bool? Success { get; set; }
        }

        private class ImgbbData
        {
            [JsonPropertyName("url")]
            public string? Url { get; set; }

            [JsonPropertyName("display_url")]
            public string? DisplayUrl { get; set; }

            [JsonPropertyName("delete_url")]
            public string? DeleteUrl { get; set; }
        }
    }

    public record ImgbbResult(string? Url, string? DisplayUrl, string? DeleteUrl);
}