using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs;
using Server.Models;
using Server.Services;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrawingsController : ControllerBase
    {
        private readonly DrawingContext _context;
        private readonly GeminiService _geminiService;

        public DrawingsController(DrawingContext context, GeminiService geminiService)
        {
            _context = context;
            _geminiService = geminiService;
        }

        [HttpPost("add-draw")]
        public async Task<IActionResult> AddDraw([FromBody] CreateDrawRequest request)
        {
            var shapes = await _geminiService.GetShapesFromPromptAsync(
                request.Prompt,
                request.ExistingDrawings
            );
            return Ok(shapes);
        }

        [HttpPost("save-canvas")]
        public async Task<IActionResult> SaveCanvas([FromBody] CanvasDto dto)
        {
            var canvas = new Canvas { Title = dto.Title, Drawings = new List<Drawing>() };

            _context.Canvases.Add(canvas);
            await _context.SaveChangesAsync();

            foreach (var drawingDto in dto.Drawings ?? new List<DrawingDto>())
            {
                var drawing = new Drawing
                {
                    Description = drawingDto.Description,
                    CanvasId = canvas.Id,
                    Shapes = new List<Shape>(),
                };

                foreach (var shapeDto in drawingDto.Shapes ?? new List<ShapeDto>())
                {
                    var shapeTypeFromDB = await _context.ShapeTypes.FirstOrDefaultAsync(s =>
                        s.Name == shapeDto.Type
                    );
                    if (shapeTypeFromDB == null)
                        throw new ArgumentException(
                            $"Shape type '{shapeDto.Type}' does not exist in the database. Please try again."
                        );

                    var shape = new Shape
                    {
                        Type = shapeTypeFromDB.Id,
                        X = shapeDto.X,
                        Y = shapeDto.Y,
                        Width = shapeDto.Width,
                        Height = shapeDto.Height,
                        Color = shapeDto.Color,
                    };

                    drawing.Shapes.Add(shape);
                }

                _context.Drawings.Add(drawing);
            }

            await _context.SaveChangesAsync();

            return Ok(new { id = canvas.Id, name = canvas.Title });
        }

        [HttpGet("{canvasId}")]
        public async Task<IActionResult> GetCanvas(int canvasId)
        {
            var canvas = await _context
                .Canvases.Include(c => c.Drawings)
                .ThenInclude(d => d.Shapes)
                .FirstOrDefaultAsync(c => c.Id == canvasId);

            if (canvas == null)
                return NotFound();
            var shapeTypes = await _context.ShapeTypes.ToListAsync();
            var result = new
            {
                canvas.Id,
                Drawings = canvas
                    .Drawings.Select(d => new
                    {
                        d.Id,
                        d.Description,
                        Shapes = d
                            .Shapes.Select(s =>
                            {
                                var shapeTypeName =
                                    shapeTypes.FirstOrDefault(t => t.Id == s.Type)?.Name
                                    ?? "Unknown";
                                return new
                                {
                                    s.X,
                                    s.Y,
                                    s.Width,
                                    s.Height,
                                    s.Color,
                                    Type = shapeTypeName,
                                };
                            })
                            .ToList(),
                    })
                    .ToList(),
            };
            return Ok(result);
        }

        [HttpGet("all-canvases")]
        public async Task<IActionResult> GetAllCanvases()
        {
            var canvases = await _context.Canvases.Select(c => new { c.Id, c.Title }).ToListAsync();

            return Ok(canvases);
        }
    }
}
