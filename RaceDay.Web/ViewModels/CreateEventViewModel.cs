using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; 

namespace RaceDay.Web.ViewModels
{
    public class CreateEventViewModel
    {
        [Required]
        public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public DateTime RegistrationDeadline { get; set; }

        
        public IFormFile? ImageFile { get; set; }
    }
}