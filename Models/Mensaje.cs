using System.Text.Json.Serialization;

namespace ChatbotAPI.Models;

public class Mensaje
{
    public long Id { get; set; }
    public int ConversacionId { get; set; }
    public string TipoEmisor { get; set; } = "USUARIO";
    public string Contenido { get; set; } = string.Empty;
    public DateTime? FechaEnvio { get; set; }

    [JsonIgnore]
    public Conversacion? Conversacion { get; set; }

    [JsonIgnore]
    public ICollection<MensajeAdjunto> Adjuntos { get; set; } = new List<MensajeAdjunto>();
}
