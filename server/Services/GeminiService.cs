using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Server.Models;
using Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Server.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly string _apiKey;
        private readonly DrawingContext _context;

        public GeminiService(HttpClient httpClient, IConfiguration config, DrawingContext context)
        {
            _httpClient = httpClient;
            _apiUrl = config["Gemini:ApiUrl"];
            _apiKey = config["Gemini:ApiKey"];
            _context = context;
        }

        public async Task<List<string>> GetSupportedShapesAsync()
        {
            return await _context.ShapeTypes
                                 .Select(s => s.Name)
                                 .ToListAsync();
        }

        public async Task<List<ShapeDto>> GetShapesFromPromptAsync(string prompt, List<DrawingDto> existingDrawings)
        {
            var supportedShapes = await GetSupportedShapesAsync();

            var supportedShapesStr = string.Join(", ", supportedShapes);

            var existingDrawingsStr = existingDrawings.Count == 0
                ? "None"
                : string.Join("\n", existingDrawings.Select(d =>
                    $"Drawing: \"{d.Description}\", Shapes: [{string.Join(", ", d.Shapes.Select(s => $"{{type: \"{s.Type}\", x: {s.X}, y: {s.Y}, width: {s.Width}, height: {s.Height}, color: \"{s.Color}\"}}"))}]"
                ));
var fullPrompt = $@"
You are an assistant that helps build a drawing for a user using only the following shapes: {supportedShapesStr}.  
The user asked: '{prompt}'.  
Currently, the following drawings already exist on the canvas: {existingDrawingsStr}.  
Each drawing is composed of multiple shapes that together form a complete object.

Your task is to intelligently decompose the requested drawing into supported shapes,  
using them in the most logical, efficient, visually balanced, and creative way possible.

Always represent each requested object as a complete composition made up of multiple shapes,  
ensuring that all important parts are included with values and placements that allow them to come together precisely, forming a perfect, coherent whole.  
Avoid oversimplified or incomplete depictions consisting of only a single shape.

The new drawing must be positioned and sized relative to the existing drawings on the canvas,  
maintaining visual harmony and spatial relationships where applicable.  
New shapes should be placed thoughtfully so that they do not obscure existing drawings unless explicitly appropriate by the request, and must remain balanced in scale and position relative to existing objects.

Each shape must be an object with the following fields:  
- type (string) — one of the supported shapes  
- x (number, can be decimal)  
- y (number, can be decimal)  
- width (number, in pixels)  
- height (number, in pixels)  
- color (string) — a valid hex code for React rendering  

---

### Coordinate System (CRITICAL):  
- Canvas origin is top-left (0,0)  
- X increases rightward, Y increases downward  

### Shape placement rules by type:  
- Circle:  
  - x, y are the center  
  - width = diameter (height must equal width)  
- Ellipse:  
  - x, y are the center  
  - width = horizontal diameter, height = vertical diameter  
- Rectangle, Square, Triangle:  
  - x, y are the top-left corner  
  - Square: width = height  
- Line:  
  - x, y are the start point  
  - width, height define offset to end point (end at x + width, y + height)  

---

### Canvas constraints:  
- Canvas size: 600 x 400 pixels  
- Shapes must be fully visible within canvas:  
  - Circle/Ellipse:   
    - x - width/2 ≥ 0  
    - x + width/2 ≤ 600  
    - y - height/2 ≥ 0  
    - y + height/2 ≤ 400  
  - Other shapes:  
    - x + width ≤ 600  
    - y + height ≤ 400  

---

### Visual and semantic guidelines:

- Related shapes should be placed near each other and follow correct vertical hierarchy:  
  - For humans:  
    - Head must be above body (lower y value)  
    - Eyes must be inside the head (head.y - head.height/2 < eye.y < head.y + head.height/2)  
    - Body starts just below head (head.y + head.height/2 + small margin)  
    - Arms attach near middle or top of body  
    - Legs attach near bottom of body  
  - For houses:  
    - Roof above walls  
  - For all ground-based objects (people, trees, houses), they must stand on the same consistent ground level — positioned at a uniform height above the bottom of the canvas (approximately y = 300–350), so that all objects appear to rest on a shared, flat ground line.

- Maintain proportional size relationships:  
  - House ≈ 4× size of a person  
  - Person ≈ 2-3× size of bush or flower  

- Symmetric parts (e.g., arms, legs, eyes) must be balanced on both sides  

- Use natural, consistent colors:  
  - Tree trunks should be light brown, with foliage and leaves in green shades.  
  - Houses should have varied colors for walls and roofs.  
  - People should have realistic skin, hair, and clothing colors.  
- Avoid flat one-color objects unless suitable  

---

### Output requirements:  
- Return **only** a raw JSON array of shape objects  
- No explanations or additional text  
- All numbers can be floats  
";



            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = fullPrompt }
                        }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var urlWithKey = $"{_apiUrl}?key={_apiKey}";
            var request = new HttpRequestMessage(HttpMethod.Post, urlWithKey)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            var cleanJson = ExtractJsonArray(text);
            var geminiShapes = JsonSerializer.Deserialize<List<ShapeDto>>(cleanJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ShapeDto>();
            var validTypes = await _context.ShapeTypes.Select(s => s.Name).ToListAsync();
            var validShapes = geminiShapes.Where(s => validTypes.Contains(s.Type)).ToList();
            return validShapes;
        }

        private string ExtractJsonArray(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "[]";

            text = text.Trim();
            if (text.StartsWith("```"))
            {
                var firstNewLine = text.IndexOf('\n');
                if (firstNewLine >= 0)
                    text = text.Substring(firstNewLine + 1);
                if (text.EndsWith("```"))
                    text = text.Substring(0, text.Length - 3);
                text = text.Trim();
            }

            int start = text.IndexOf('[');
            int end = text.LastIndexOf(']');
            if (start >= 0 && end > start)
                return text.Substring(start, end - start + 1);

            return text;
        }
    }
}
