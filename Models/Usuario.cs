using System.Text.Json.Serialization;

namespace ChatbotAPI.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime? FechaRegistro { get; set; }

    [JsonIgnore]
    public ICollection<Conversacion> Conversaciones { get; set; } = new List<Conversacion>();
}
