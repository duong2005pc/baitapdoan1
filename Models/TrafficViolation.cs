using System.ComponentModel.DataAnnotations;

namespace TrafficLaw.API.Models
{
    public class TrafficViolation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string QaId { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Question { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string VehicleType { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Keywords { get; set; }

        [Required]
        [MaxLength(500)]
        public string FineText { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? PointsDeducted { get; set; }

        [MaxLength(500)]
        public string? AdditionalPenalty { get; set; }

        public ICollection<LegalCitation> Citations { get; set; } = new List<LegalCitation>();
    }
}