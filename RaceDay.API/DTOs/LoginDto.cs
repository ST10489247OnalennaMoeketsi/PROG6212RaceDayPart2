using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    
    /// <summary>
    /// Data Transfer Object for user login credentials.
    /// </summary>
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
