using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Server.Models
{
    public class DrawingDto
    {
        public string Description { get; set; } = "";
        public List<ShapeDto> Shapes { get; set; } = new();
    }
}