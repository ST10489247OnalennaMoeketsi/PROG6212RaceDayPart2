using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    /// <summary> 
    /// Represents a race event created by an Organiser.
    /// </summary>
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public int OrganiserId { get; set; }

        [ForeignKey("OrganiserId")]
        public User? Organiser { get; set; }

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
        public string Status { get; set; } = "Open"; // Draft, Open, Closed, Completed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property for the Many-to-Many relationship
        public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
    }
}
