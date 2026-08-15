using ChatbotAPI.Data;
using ChatbotAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatbotAPI.Services;

public class ChatService
{
    private readonly ApplicationDbContext _context;
    private readonly OllamaService _ollama;

    public ChatService(ApplicationDbContext context, OllamaService ollama)
    {
        _context = context;
        _ollama = ollama;
    }

    public async Task<ChatResponse> EnviarMensajeAsync(ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Mensaje))
        {
            throw new InvalidOperationException("El mensaje es obligatorio.");
        }

        var conversacion = await ObtenerOCrearConversacionAsync(request.UsuarioId, request.ConversacionId);

        var historial = await _context.Mensajes
            .Where(m => m.ConversacionId == conversacion.Id)
            .OrderBy(m => m.FechaEnvio)
            .ToListAsync();

        var mensajeUsuario = new Mensaje
        {
            ConversacionId = conversacion.Id,
            TipoEmisor = "USUARIO",
            Contenido = request.Mensaje,
            FechaEnvio = DateTime.Now
        };

        _context.Mensajes.Add(mensajeUsuario);

        var respuesta = await _ollama.GenerarRespuestaConTextoYContexto(historial, request.Mensaje);

        var mensajeSistema = new Mensaje
        {
            ConversacionId = conversacion.Id,
            TipoEmisor = "SISTEMA",
            Contenido = respuesta,
            FechaEnvio = DateTime.Now
        };

        _context.Mensajes.Add(mensajeSistema);
        await _context.SaveChangesAsync();

        return new ChatResponse
        {
            Mensaje = request.Mensaje,
            Respuesta = respuesta,
            Remitente = "SISTEMA",
            ConversacionId = conversacion.Id,
            UsuarioId = conversacion.UsuarioId
        };
    }

    public async Task<ChatResponse> EnviarImagenAsync(ChatImagenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Mensaje) && string.IsNullOrWhiteSpace(request?.ImagenBase64))
        {
            throw new InvalidOperationException("Debes enviar un mensaje o una imagen.");
        }

        var contenido = string.IsNullOrWhiteSpace(request.Mensaje)
            ? "Usuario envío una imagen."
            : request.Mensaje;

        var conversacion = await ObtenerOCrearConversacionAsync(request.UsuarioId, request.ConversacionId);

        var mensajeUsuario = new Mensaje
        {
            ConversacionId = conversacion.Id,
            TipoEmisor = "USUARIO",
            Contenido = contenido,
            FechaEnvio = DateTime.Now
        };

        _context.Mensajes.Add(mensajeUsuario);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(request.ImagenBase64))
        {
            var archivoBase64 = request.ImagenBase64;
            if (archivoBase64.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var index = archivoBase64.IndexOf(",", StringComparison.Ordinal);
                if (index >= 0)
                {
                    archivoBase64 = archivoBase64[(index + 1)..];
                }
            }

            var adjunto = new MensajeAdjunto
            {
                MensajeId = mensajeUsuario.Id,
                ConversacionId = conversacion.Id,
                TipoArchivo = "IMAGEN",
                MimeType = "image/png",
                NombreArchivo = $"imagen_{mensajeUsuario.Id}.png",
                Base64 = archivoBase64,
                FechaCreacion = DateTime.Now
            };

            _context.MensajesAdjuntos.Add(adjunto);
        }

        var historial = await _context.Mensajes
            .Where(m => m.ConversacionId == conversacion.Id)
            .OrderBy(m => m.FechaEnvio)
            .ToListAsync();

        var respuesta = await _ollama.GenerarRespuestaConImagenYContexto(historial, request.Mensaje, request.ImagenBase64);

        var mensajeSistema = new Mensaje
        {
            ConversacionId = conversacion.Id,
            TipoEmisor = "SISTEMA",
            Contenido = respuesta,
            FechaEnvio = DateTime.Now
        };

        _context.Mensajes.Add(mensajeSistema);
        await _context.SaveChangesAsync();

        return new ChatResponse
        {
            Mensaje = contenido,
            Respuesta = respuesta,
            Remitente = "SISTEMA",
            ConversacionId = conversacion.Id,
            UsuarioId = conversacion.UsuarioId
        };
    }

    public async Task<Conversacion> ObtenerOCrearConversacionAsync(int usuarioId, int conversacionId)
    {
        if (conversacionId > 0)
        {
            var conversacionExistente = await _context.Conversaciones
                .FirstOrDefaultAsync(c => c.Id == conversacionId);

            if (conversacionExistente == null)
            {
                throw new InvalidOperationException("La conversación indicada no existe.");
            }

            return conversacionExistente;
        }

        if (usuarioId <= 0)
        {
            throw new InvalidOperationException("Debes enviar usuarioId o conversacionId para guardar la conversación.");
        }

        var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == usuarioId);
        if (!usuarioExiste)
        {
            throw new InvalidOperationException("El usuario indicado no existe.");
        }

        var conversacion = await _context.Conversaciones
            .OrderByDescending(c => c.FechaActualizacion ?? c.FechaCreacion)
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        if (conversacion != null)
        {
            conversacion.FechaActualizacion = DateTime.Now;
            await _context.SaveChangesAsync();
            return conversacion;
        }

        var nuevaConversacion = new Conversacion
        {
            UsuarioId = usuarioId,
            Titulo = "Chat con IA",
            FechaCreacion = DateTime.Now,
            FechaActualizacion = DateTime.Now
        };

        _context.Conversaciones.Add(nuevaConversacion);
        await _context.SaveChangesAsync();
        return nuevaConversacion;
    }
}

public class ChatRequest
{
    public string Mensaje { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public int ConversacionId { get; set; }
}

public class ChatImagenRequest
{
    public string Mensaje { get; set; } = string.Empty;
    public string? ImagenBase64 { get; set; }
    public int UsuarioId { get; set; }
    public int ConversacionId { get; set; }
}

public class ChatResponse
{
    public string Mensaje { get; set; } = string.Empty;
    public string Respuesta { get; set; } = string.Empty;
    public string Remitente { get; set; } = string.Empty;
    public int ConversacionId { get; set; }
    public int UsuarioId { get; set; }
}
