using System.Collections.Generic;
using Server.Models;
using System.Text.Json.Serialization;

namespace Server.DTOs
{
    public class CreateDrawRequest
    {
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = "";

        [JsonPropertyName("existingDrawings")]
        public List<DrawingDto> ExistingDrawings { get; set; } = new();
    }
}
