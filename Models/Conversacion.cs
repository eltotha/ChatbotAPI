using System.Text.Json.Serialization;

namespace ChatbotAPI.Models;

public class Conversacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string? Titulo { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }

    [JsonIgnore]
    public Usuario? Usuario { get; set; }

    [JsonIgnore]
    public ICollection<Mensaje> Mensajes { get; set; } = new List<Mensaje>();
}
