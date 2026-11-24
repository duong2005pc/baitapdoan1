using System.ComponentModel.DataAnnotations;

namespace TrafficLaw.API.Models
{
    public class ChatRequestDto
    {
        [Required]
        [StringLength(4000, MinimumLength = 1)]
        public string Question { get; set; } = string.Empty;
    }
}