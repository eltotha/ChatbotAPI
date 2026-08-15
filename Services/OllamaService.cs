using System.Net.Http.Json;
using ChatbotAPI.Models;

namespace ChatbotAPI.Services;

public class OllamaService
{
    private readonly HttpClient _httpClient;

    public OllamaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GenerarRespuesta(string mensaje)
    {
        if (string.IsNullOrWhiteSpace(mensaje))
        {
            return string.Empty;
        }

        var request = new
        {
            model = "medgemma:4b",
            prompt = mensaje,
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync(
            "http://localhost:11434/api/generate",
            request
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Error al consultar Ollama: {response.StatusCode}. {error}");
        }

        var resultado = await response.Content.ReadFromJsonAsync<OllamaResponse>();
        return resultado?.response ?? string.Empty;
    }

    public async Task<string> GenerarRespuestaConTextoYContexto(List<Mensaje> historial, string nuevoMensaje)
    {
        if (string.IsNullOrWhiteSpace(nuevoMensaje))
        {
            return string.Empty;
        }

        var prompt = ConstruirPromptConContexto(historial, nuevoMensaje);
        return await GenerarRespuesta(prompt);
    }

    public async Task<string> GenerarRespuestaConImagenYContexto(List<Mensaje> historial, string nuevoMensaje, string? imagenBase64)
    {
        var texto = string.IsNullOrWhiteSpace(nuevoMensaje) ? "Describe esta imagen." : nuevoMensaje;

        if (string.IsNullOrWhiteSpace(imagenBase64))
        {
            return await GenerarRespuestaConTextoYContexto(historial, texto);
        }

        var base64 = NormalizarBase64Imagen(imagenBase64);
        var prompt = ConstruirPromptConContexto(historial, texto);

        var request = new
        {
            model = "medgemma:4b",
            prompt = prompt,
            images = new[] { base64 },
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync(
            "http://localhost:11434/api/generate",
            request
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Error al consultar Ollama con imagen: {response.StatusCode}. {error}");
        }

        var resultado = await response.Content.ReadFromJsonAsync<OllamaResponse>();
        return resultado?.response ?? string.Empty;
    }

    public async Task<string> GenerarRespuestaConImagen(string mensaje, string? imagenBase64)
    {
        var texto = string.IsNullOrWhiteSpace(mensaje) ? "Describe esta imagen." : mensaje;

        if (string.IsNullOrWhiteSpace(imagenBase64))
        {
            return await GenerarRespuesta(texto);
        }

        var base64 = NormalizarBase64Imagen(imagenBase64);

        var request = new
        {
            model = "medgemma:4b",
            prompt = texto,
            images = new[] { base64 },
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync(
            "http://localhost:11434/api/generate",
            request
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Error al consultar Ollama con imagen: {response.StatusCode}. {error}");
        }

        var resultado = await response.Content.ReadFromJsonAsync<OllamaResponse>();
        return resultado?.response ?? string.Empty;
    }

    private static string ConstruirPromptConContexto(List<Mensaje> historial, string nuevoMensaje)
    {
        var mensajes = historial
            .OrderBy(m => m.FechaEnvio)
            .Select(m => $"{m.TipoEmisor}: {m.Contenido}")
            .ToList();

        var prompt = new List<string>
        {
            "Eres un asistente útil y conservas el contexto de la conversación.",
            "Responde en español y mantén la conversación coherente con el historial anterior."
        };

        prompt.AddRange(mensajes);
        prompt.Add($"USUARIO: {nuevoMensaje}");
        prompt.Add("ASISTENTE:");

        return string.Join(Environment.NewLine, prompt);
    }

    private static string NormalizarBase64Imagen(string imagenBase64)
    {
        var base64 = imagenBase64.Trim();

        if (base64.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var index = base64.IndexOf(",", StringComparison.Ordinal);
            if (index >= 0)
            {
                base64 = base64[(index + 1)..];
            }
        }

        base64 = base64.Replace(" ", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);

        if (string.IsNullOrWhiteSpace(base64) || !Convert.TryFromBase64String(base64, new Span<byte>(new byte[base64.Length]), out _))
        {
            throw new InvalidOperationException("La imagen enviada no es un Base64 válido para Ollama. Asegúrate de enviar una imagen en formato data URL o base64 plano.");
        }

        return base64;
    }
}

public class OllamaResponse
{
    public string response { get; set; } = string.Empty;
}