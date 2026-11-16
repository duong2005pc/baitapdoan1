using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TrafficLaw.API.Models;

namespace TrafficLaw.API.Data
{
    public class ViolationJsonDto
    {
        [JsonPropertyName("qa_id")]
        public string QaId { get; set; } = string.Empty;

        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_type")]
        public string VehicleType { get; set; } = string.Empty;

        [JsonPropertyName("fine_text")]
        public string FineText { get; set; } = string.Empty;

        [JsonPropertyName("points_deducted")]
        public string? PointsDeducted { get; set; }

        [JsonPropertyName("additional_penalty")]
        public string? AdditionalPenalty { get; set; }

        [JsonPropertyName("citations")]
        public List<CitationJsonDto> Citations { get; set; } = new();
    }

    public class CitationJsonDto
    {
        [JsonPropertyName("document")]
        public string Document { get; set; } = string.Empty;

        [JsonPropertyName("article")]
        public string Article { get; set; } = string.Empty;

        [JsonPropertyName("clause")]
        public string? Clause { get; set; }

        [JsonPropertyName("point")]
        public string? Point { get; set; }
    }

    public class DataSeeder
    {
        public static async Task SeedFromJsonAsync(AppDbContext context, string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
                throw new FileNotFoundException($"Không tìm thấy file dữ liệu: {jsonFilePath}");

            var jsonString = await File.ReadAllTextAsync(jsonFilePath);
            var items = JsonSerializer.Deserialize<List<ViolationJsonDto>>(jsonString);

            if (items == null || !items.Any()) return;

            foreach (var item in items)
            {
                if (item.VehicleType == "khong_xac_dinh" || !item.Citations.Any())
                    continue;

                var exists = await context.TrafficViolations
                    .AnyAsync(v => v.QaId == item.QaId);

                if (!exists)
                {
                    var violation = new TrafficViolation
                    {
                        QaId = item.QaId,
                        Question = item.Question,
                        VehicleType = item.VehicleType,
                        FineText = item.FineText,
                        PointsDeducted = item.PointsDeducted,
                        AdditionalPenalty = item.AdditionalPenalty,
                        Citations = item.Citations.Select(c => new LegalCitation
                        {
                            DocumentName = c.Document,
                            Article = c.Article,
                            Clause = c.Clause,
                            Point = c.Point
                        }).ToList()
                    };

                    context.TrafficViolations.Add(violation);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}