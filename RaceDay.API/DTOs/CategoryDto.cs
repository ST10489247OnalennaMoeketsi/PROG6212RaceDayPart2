using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    
    /// <summary>
    /// Data Transfer Object for creating and updating categories.
    /// </summary>
    public class CategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }
    }
}
