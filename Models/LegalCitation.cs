using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficLaw.API.Models
{
    public class LegalCitation
    {
        [Key]
        public int Id { get; set; }

        public int ViolationId { get; set; }

        [ForeignKey(nameof(ViolationId))]
        public TrafficViolation? Violation { get; set; }

        [Required]
        [MaxLength(100)]
        public string DocumentName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Article { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Clause { get; set; }

        [MaxLength(50)]
        public string? Point { get; set; }
    }
}