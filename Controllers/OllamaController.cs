using ChatbotAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotAPI.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;

    public ChatController(ChatService chatService)
    {
        _chatService = chatService;
    }

    // POST: /api/chat
    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        try
        {
            var respuesta = await _chatService.EnviarMensajeAsync(request);
            return Ok(new
            {
                mensaje = respuesta.Mensaje,
                respuesta = respuesta.Respuesta,
                remitente = respuesta.Remitente,
                conversacionId = respuesta.ConversacionId,
                usuarioId = respuesta.UsuarioId
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // POST: /api/chat/imagen
    [HttpPost("imagen")]
    public async Task<IActionResult> ChatConImagen([FromBody] ChatImagenRequest request)
    {
        try
        {
            var respuesta = await _chatService.EnviarImagenAsync(request);
            return Ok(new
            {
                mensaje = respuesta.Mensaje,
                respuesta = respuesta.Respuesta,
                remitente = respuesta.Remitente,
                conversacionId = respuesta.ConversacionId,
                usuarioId = respuesta.UsuarioId
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}