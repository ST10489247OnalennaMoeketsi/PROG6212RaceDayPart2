using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int EnrolmentId { get; set; }

        [ForeignKey("EnrolmentId")]
        public Enrolment? Enrolment { get; set; }

        // TimeSpan maps to SQL TIME
        public TimeSpan? FinishTime { get; set; }

        public int? Position { get; set; }

        [Required]
        [StringLength(30)]
        public string ResultStatus { get; set; } = "Finished"; // Finished, DNF, DNS, Disqualified

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
