using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    // A Participant only needs to send which Category they want to enrol in.
    // The EventId comes from the URL, and the ParticipantId comes from the JWT token.
    public class EnrolmentDto
    {
        [Required]
        public int CategoryId { get; set; }
    }
}
