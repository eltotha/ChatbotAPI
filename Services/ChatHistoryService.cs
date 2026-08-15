using ChatbotAPI.Data;
using ChatbotAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatbotAPI.Services;

public class ChatHistoryService
{
    private readonly ApplicationDbContext _context;

    public ChatHistoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HistorialChatResponse> ObtenerHistorialAsync(int conversacionId)
    {
        var conversacionExiste = await _context.Conversaciones.AnyAsync(c => c.Id == conversacionId);
        if (!conversacionExiste)
        {
            throw new InvalidOperationException("La conversación indicada no existe.");
        }

        var mensajes = await _context.Mensajes
            .Where(m => m.ConversacionId == conversacionId)
            .OrderBy(m => m.FechaEnvio)
            .Select(m => new HistorialMensajeDto
            {
                Id = m.Id,
                ConversacionId = m.ConversacionId,
                TipoEmisor = m.TipoEmisor,
                Contenido = m.Contenido,
                FechaEnvio = m.FechaEnvio,
                Adjuntos = m.Adjuntos.Select(a => new HistorialAdjuntoDto
                {
                    Id = a.Id,
                    TipoArchivo = a.TipoArchivo,
                    MimeType = a.MimeType,
                    NombreArchivo = a.NombreArchivo,
                    Base64 = a.Base64,
                    FechaCreacion = a.FechaCreacion
                }).ToList()
            })
            .ToListAsync();

        return new HistorialChatResponse
        {
            ConversacionId = conversacionId,
            Mensajes = mensajes
        };
    }

    public async Task<List<Mensaje>> ObtenerHistorialConversacionParaContextoAsync(int conversacionId)
    {
        return await _context.Mensajes
            .Where(m => m.ConversacionId == conversacionId)
            .OrderBy(m => m.FechaEnvio)
            .ToListAsync();
    }
}

public class HistorialChatResponse
{
    public int ConversacionId { get; set; }
    public List<HistorialMensajeDto> Mensajes { get; set; } = new();
}

public class HistorialMensajeDto
{
    public long Id { get; set; }
    public int ConversacionId { get; set; }
    public string TipoEmisor { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public DateTime? FechaEnvio { get; set; }
    public List<HistorialAdjuntoDto> Adjuntos { get; set; } = new();
}

public class HistorialAdjuntoDto
{
    public long Id { get; set; }
    public string TipoArchivo { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public string? NombreArchivo { get; set; }
    public string? Base64 { get; set; }
    public DateTime? FechaCreacion { get; set; }
}
