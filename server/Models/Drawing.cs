using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Server.Models
{
    public class Drawing
    {
        public int Id { get; set; }
        [Required]
        public int CanvasId { get; set; }
        [JsonIgnore]
        public Canvas? Canvas { get; set; }
        public string Description { get; set; } = "";
        public List<Shape> Shapes { get; set; } = new();
    }
}
