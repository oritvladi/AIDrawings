using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data
{
    public class DrawingContext : DbContext
    {
        public DrawingContext(DbContextOptions<DrawingContext> options) : base(options) { }

        public DbSet<Drawing> Drawings { get; set; }
        public DbSet<Shape> Shapes { get; set; }
        public DbSet<ShapeType> ShapeTypes { get; set; }
        public DbSet<Canvas> Canvases { get; set; }
    }
}
