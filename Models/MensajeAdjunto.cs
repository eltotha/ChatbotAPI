using System.Text.Json.Serialization;

namespace ChatbotAPI.Models;

public class MensajeAdjunto
{
    public long Id { get; set; }
    public long MensajeId { get; set; }
    public int ConversacionId { get; set; }
    public string TipoArchivo { get; set; } = "IMAGEN";
    public string? MimeType { get; set; }
    public string? NombreArchivo { get; set; }
    public string? Base64 { get; set; }
    public DateTime? FechaCreacion { get; set; }

    [JsonIgnore]
    public Mensaje Mensaje { get; set; } = null!;

    [JsonIgnore]
    public Conversacion Conversacion { get; set; } = null!;
}
