using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    
    public class EventDto
    {
        [Required]
        [StringLength(150)]
        public string EventName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public DateTime RegistrationDeadline { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Open";
    }
}