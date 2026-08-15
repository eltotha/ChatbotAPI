using ChatbotAPI.Data;
using ChatbotAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatbotAPI.Controllers;

// Ruta base: /api/conversaciones
// Gestiona la creación, consulta y eliminación de conversaciones por usuario.
[ApiController]
[Route("api/[controller]")]
public class ConversacionesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ConversacionesController(ApplicationDbContext context)
    {
        _context = context;
    }

    //GET: /api/conversaciones
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Conversacion>>> GetConversaciones()
    {
        return await _context.Conversaciones
            .Include(c => c.Usuario)
            .Include(c => c.Mensajes)
            .ToListAsync();
    }

    //GET: /api/conversaciones/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Conversacion>> GetConversacion(int id)
    {
        var conversacion = await _context.Conversaciones
            .Include(c => c.Usuario)
            .Include(c => c.Mensajes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (conversacion == null)
        {
            return NotFound();
        }

        return conversacion;
    }

    //GET: /api/conversaciones/usuario/{usuarioId}
    [HttpGet("usuario/{usuarioId:int}")]
    public async Task<ActionResult<IEnumerable<Conversacion>>> GetConversacionesPorUsuario(int usuarioId)
    {
        var conversaciones = await _context.Conversaciones
            .Where(c => c.UsuarioId == usuarioId)
            .Include(c => c.Mensajes)
            .ToListAsync();

        return conversaciones;
    }

   //POST: /api/conversaciones
    [HttpPost]
    public async Task<ActionResult<Conversacion>> PostConversacion(Conversacion conversacion)
    {
        if (conversacion.UsuarioId <= 0)
        {
            return BadRequest("El usuario_id es obligatorio.");
        }

        var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == conversacion.UsuarioId);
        if (!usuarioExiste)
        {
            return BadRequest("El usuario indicado no existe.");
        }

        _context.Conversaciones.Add(conversacion);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetConversacion), new { id = conversacion.Id }, conversacion);
    }

    //PUT: /api/conversaciones/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutConversacion(int id, Conversacion conversacion)
    {
        if (id != conversacion.Id)
        {
            return BadRequest();
        }

        _context.Entry(conversacion).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ConversacionExists(id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    //DELETE: /api/conversaciones/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteConversacion(int id)
    {
        var conversacion = await _context.Conversaciones.FindAsync(id);
        if (conversacion == null)
        {
            return NotFound();
        }

        _context.Conversaciones.Remove(conversacion);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ConversacionExists(int id)
    {
        return _context.Conversaciones.Any(e => e.Id == id);
    }
}
