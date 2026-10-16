using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class ResultDto
    {
        [Required]
        public int EnrolmentId { get; set; }

        // TimeSpan maps to the SQL TIME type (e.g., "00:52:34")
        public TimeSpan? FinishTime { get; set; }

        // Position must be greater than 0 or null
        [Range(1, int.MaxValue, ErrorMessage = "Position must be greater than 0.")]
        public int? Position { get; set; }

        [Required]
        [StringLength(30)]
        public string ResultStatus { get; set; } = "Finished";
    }
}
