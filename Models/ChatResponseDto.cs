using System.Text.Json.Serialization;

namespace TrafficLaw.API.Models
{
    public class ChatResponseDto
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "answered";

        [JsonPropertyName("answer")]
        public string Answer { get; set; } = string.Empty;

        [JsonPropertyName("fine_text")]
        public string? FineText { get; set; }

        [JsonPropertyName("points_deducted")]
        public string? PointsDeducted { get; set; }

        [JsonPropertyName("additional_penalty")]
        public string? AdditionalPenalty { get; set; }

        [JsonPropertyName("citations")]
        public List<CitationItemDto> Citations { get; set; } = new();
    }

    public class CitationItemDto
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
}