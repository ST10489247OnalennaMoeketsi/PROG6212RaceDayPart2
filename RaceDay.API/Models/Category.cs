using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    /// <summary>
    /// Represents a reusable race category
    /// </summary>
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        // Navigation Property for the Many-to-Many relationship
        public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
    }
}
