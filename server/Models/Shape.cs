using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Server.Models
{
    public class ShapeType
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class Shape
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [JsonPropertyName("type")]
        public int Type { get; set; }

        [Required]
        [JsonPropertyName("x")]
        public double X { get; set; }

        [Required]
        [JsonPropertyName("y")]
        public double Y { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("drawingId")]
        public int DrawingId { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; } = "";

        [JsonIgnore]
        public Drawing? Drawing { get; set; }
    }
}
