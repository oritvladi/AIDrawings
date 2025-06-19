using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Canvas
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = "";

        public List<Drawing> Drawings { get; set; } = new();
    }
}
