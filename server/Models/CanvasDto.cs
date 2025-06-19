using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class CanvasDto
    {
        public string Title { get; set; } = "";
        public List<DrawingDto> Drawings { get; set; }
    }
}
