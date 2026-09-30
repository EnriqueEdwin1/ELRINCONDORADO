using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ELRINCONDORADO.Helpers
{
    // Subida de imágenes a ImgBB desde el servidor.
    //
    // Acepta cualquier tipo de imagen: no hay lista blanca de formatos. Cuando el
    // navegador no declara un Content-Type válido (archivos con 'octet-stream',
    // renombrados, HEIC/AVIF, etc.), se identifica la firma real del archivo
    // (magic bytes) para que ImgBB reciba el tipo MIME correcto. Si ImgBB igual
    // rechaza el archivo, el error devuelto llega traducido en 'Error' para que
    // el panel de administración pueda mostrarlo.
    //
    // La clave NO se guarda en appsettings (quedó expuesta en el pasado): se lee
    // de configuración (user-secrets o variable de entorno Imgbb__ApiKey).
    public static class ImgbbHelper
    {
        private const string UploadUrl = "https://api.imgbb.com/1/upload";
        private const long MaximoBytes = 32L * 1024 * 1024; // Límite de ImgBB: 32 MB

        private static readonly HttpClient _http = new();

        public static async Task<ImgbbResult> SubirImagenAsync(IFormFile imagen, string? apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return ImgbbResult.Fallo(
                    "La clave de ImgBB no está configurada. Configúrala con 'dotnet user-secrets set Imgbb:ApiKey TU_CLAVE' " +
                    "(o con la variable de entorno Imgbb__ApiKey) y vuelve a intentarlo.");

            if (imagen == null || imagen.Length == 0)
                return ImgbbResult.Fallo("No se recibió ningún archivo de imagen.");

            if (imagen.Length > MaximoBytes)
                return ImgbbResult.Fallo("La imagen supera el tamaño máximo permitido por ImgBB (32 MB).");

            using var ms = new MemoryStream();
            await imagen.CopyToAsync(ms);
            var bytes = ms.ToArray();

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(apiKey), "key");

            var contenido = new ByteArrayContent(bytes);
            var contentType = ObtenerContentType(imagen.ContentType, bytes);
            contenido.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            var nombreArchivo = string.IsNullOrWhiteSpace(imagen.FileName)
                ? $"imagen-{DateTime.Now:yyyyMMddHHmmss}.{ExtensionPara(contentType)}"
                : imagen.FileName;
            form.Add(contenido, "image", nombreArchivo);

            try
            {
                var respuesta = await _http.PostAsync(UploadUrl, form);
                var cuerpo = await respuesta.Content.ReadAsStringAsync();

                var resultado = JsonSerializer.Deserialize<ImgbbResponse>(
                    cuerpo,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!respuesta.IsSuccessStatusCode ||
                    resultado?.Data == null ||
                    resultado.Success != true)
                {
                    return ImgbbResult.Fallo(ExtraerError(resultado, respuesta.StatusCode));
                }

                return new ImgbbResult(
                    Url: resultado.Data.Url,
                    DisplayUrl: resultado.Data.DisplayUrl,
                    DeleteUrl: resultado.Data.DeleteUrl,
                    Error: null);
            }
            catch (Exception ex)
            {
                return ImgbbResult.Fallo($"No se pudo contactar con ImgBB: {ex.Message}");
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

        // El navegador suele mandar 'image/png' o 'image/jpeg' bien declarados,
        // pero algunos archivos (o formatos como HEIC/AVIF) llegan como
        // 'application/octet-stream' o con el tipo vacío. En ese caso el formato
        // se identifica por la firma real del archivo para que ImgBB lo reconozca.
        private static string ObtenerContentType(string? contentType, byte[] bytes)
        {
            if (!string.IsNullOrWhiteSpace(contentType) &&
                contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return contentType;
            }

            return DetectarContentType(bytes) ?? "application/octet-stream";
        }

        private static string? DetectarContentType(byte[] b)
        {
            // PNG: 89 50 4E 47
            if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47)
                return "image/png";

            // JPEG: FF D8 FF
            if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
                return "image/jpeg";

            // GIF87a / GIF89a
            if (b.Length >= 6 && b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46 &&
                b[3] == 0x38 && (b[4] == 0x39 || b[4] == 0x37) && b[5] == 0x61)
                return "image/gif";

            // WEBP: RIFF....WEBP
            if (b.Length >= 12 && b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46 &&
                b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50)
                return "image/webp";

            // BMP: BM
            if (b.Length >= 2 && b[0] == 0x42 && b[1] == 0x4D)
                return "image/bmp";

            // TIFF (little / big endian)
            if (b.Length >= 4 &&
                ((b[0] == 0x49 && b[1] == 0x49 && b[2] == 0x2A && b[3] == 0x00) ||
                 (b[0] == 0x4D && b[1] == 0x4D && b[2] == 0x00 && b[3] == 0x2A)))
                return "image/tiff";

            // ICO: 00 00 01 00
            if (b.Length >= 4 && b[0] == 0x00 && b[1] == 0x00 && b[2] == 0x01 && b[3] == 0x00)
                return "image/x-icon";

            // AVIF / HEIC usan contenedor ISO BMFF ('ftyp' en los bytes 4..7).
            if (b.Length >= 12 && b[4] == (byte)'f' && b[5] == (byte)'t' && b[6] == (byte)'y' && b[7] == (byte)'p')
            {
                var marca = Encoding.ASCII.GetString(b, 8, 4);
                if (marca.Equals("avif", StringComparison.OrdinalIgnoreCase) ||
                    marca.Equals("avis", StringComparison.OrdinalIgnoreCase))
                    return "image/avif";

                if (marca.StartsWith("heic", StringComparison.OrdinalIgnoreCase) ||
                    marca.StartsWith("heix", StringComparison.OrdinalIgnoreCase) ||
                    marca == "mif1" || marca == "msf1")
                    return "image/heic";
            }

            // SVG: XML/HTML que comienza (tras espacios) en '<svg'.
            if (b.Length >= 5)
            {
                var inicio = 0;
                while (inicio < b.Length && (b[inicio] == (byte)' ' || b[inicio] == (byte)'\t' ||
                                             b[inicio] == (byte)'\r' || b[inicio] == (byte)'\n'))
                    inicio++;

                var longitud = Math.Min(128, b.Length - inicio);
                if (longitud > 0)
                {
                    var cabeza = Encoding.ASCII.GetString(b, inicio, longitud);
                    if (cabeza.Contains("<svg", StringComparison.OrdinalIgnoreCase))
                        return "image/svg+xml";
                }
            }

            return null;
        }

        private static string ExtensionPara(string contentType) => contentType switch
        {
            "image/png" => "png",
            "image/jpeg" => "jpg",
            "image/gif" => "gif",
            "image/webp" => "webp",
            "image/bmp" => "bmp",
            "image/tiff" => "tiff",
            "image/avif" => "avif",
            "image/heic" => "heic",
            "image/svg+xml" => "svg",
            _ => "img"
        };

        // Traduce el rechazo de ImgBB. El mensaje original viene en
        // 'error.message'; si no, se intenta dar una pista útil.
        private static string ExtraerError(ImgbbResponse? resultado, System.Net.HttpStatusCode status)
        {
            if (!string.IsNullOrWhiteSpace(resultado?.Error?.Message))
                return resultado.Error.Message;

            return status switch
            {
                System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden =>
                    "ImgBB rechazó la solicitud: revisa que la clave de API sea correcta.",
                System.Net.HttpStatusCode.RequestEntityTooLarge =>
                    "La imagen supera el tamaño máximo permitido por ImgBB (32 MB).",
                System.Net.HttpStatusCode.UnsupportedMediaType =>
                    "ImgBB no admite el formato de esta imagen. Pruébala convertida a JPG, PNG, GIF, WEBP, BMP o TIFF.",
                _ => "ImgBB no pudo procesar la imagen. Revisa que el archivo sea una imagen válida."
            };
        }

        private class ImgbbResponse
        {
            [JsonPropertyName("data")]
            public ImgbbData? Data { get; set; }

            [JsonPropertyName("success")]
            public bool? Success { get; set; }

            [JsonPropertyName("error")]
            public ImgbbError? Error { get; set; }
        }

        private class ImgbbError
        {
            [JsonPropertyName("message")]
            public string? Message { get; set; }
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

    public record ImgbbResult(string? Url, string? DisplayUrl, string? DeleteUrl, string? Error)
    {
        public bool Exitoso => string.IsNullOrEmpty(Error) && !string.IsNullOrEmpty(Url);

        public static ImgbbResult Fallo(string error) => new(null, null, null, error);
    }
}