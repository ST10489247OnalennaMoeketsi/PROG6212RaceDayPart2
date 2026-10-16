using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class UserProfileDto
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }
    }
}