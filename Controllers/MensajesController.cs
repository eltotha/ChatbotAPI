using ChatbotAPI.Data;
using ChatbotAPI.Models;
using ChatbotAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatbotAPI.Controllers;

// Ruta base: /api/mensajes
// Gestiona el almacenamiento y consulta de mensajes dentro de cada conversación.
[ApiController]
[Route("api/[controller]")]
public class MensajesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ChatHistoryService _chatHistoryService;

    public MensajesController(ApplicationDbContext context, ChatHistoryService chatHistoryService)
    {
        _context = context;
        _chatHistoryService = chatHistoryService;
    }

    //GET: /api/mensajes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Mensaje>>> GetMensajes()
    {
        return await _context.Mensajes
            .Include(m => m.Conversacion)
            .ToListAsync();
    }

    //GET: /api/mensajes/{id}
    [HttpGet("{id:long}")]
    public async Task<ActionResult<Mensaje>> GetMensaje(long id)
    {
        var mensaje = await _context.Mensajes
            .Include(m => m.Conversacion)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (mensaje == null)
        {
            return NotFound();
        }

        return mensaje;
    }

    //GET: /api/mensajes/conversacion/{conversacionId}
    [HttpGet("conversacion/{conversacionId:int}")]
    public async Task<ActionResult<IEnumerable<Mensaje>>> GetMensajesPorConversacion(int conversacionId)
    {
        var mensajes = await _context.Mensajes
            .Where(m => m.ConversacionId == conversacionId)
            .OrderBy(m => m.FechaEnvio)
            .ToListAsync();

        return mensajes;
    }

    //GET: /api/mensajes/historial/{conversacionId}
    [HttpGet("historial/{conversacionId:int}")]
    public async Task<ActionResult<HistorialChatResponse>> GetHistorialCompleto(int conversacionId)
    {
        try
        {
            var historial = await _chatHistoryService.ObtenerHistorialAsync(conversacionId);
            return Ok(historial);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    //POST: /api/mensajes
    [HttpPost]
    public async Task<ActionResult<Mensaje>> PostMensaje(Mensaje mensaje)
    {
        if (mensaje.ConversacionId <= 0)
        {
            return BadRequest("La conversación es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(mensaje.Contenido))
        {
            return BadRequest("El contenido del mensaje es obligatorio.");
        }

        var conversacionExiste = await _context.Conversaciones.AnyAsync(c => c.Id == mensaje.ConversacionId);
        if (!conversacionExiste)
        {
            return BadRequest("La conversación indicada no existe.");
        }

        _context.Mensajes.Add(mensaje);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMensaje), new { id = mensaje.Id }, mensaje);
    }

    //PUT: /api/mensajes/{id}
    [HttpPut("{id:long}")]
    public async Task<IActionResult> PutMensaje(long id, Mensaje mensaje)
    {
        if (id != mensaje.Id)
        {
            return BadRequest();
        }

        _context.Entry(mensaje).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MensajeExists(id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    //DELETE: /api/mensajes/{id}
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteMensaje(long id)
    {
        var mensaje = await _context.Mensajes.FindAsync(id);
        if (mensaje == null)
        {
            return NotFound();
        }

        _context.Mensajes.Remove(mensaje);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool MensajeExists(long id)
    {
        return _context.Mensajes.Any(e => e.Id == id);
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
