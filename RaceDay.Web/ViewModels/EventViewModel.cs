namespace RaceDay.Web.ViewModels
{
    public class EventViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        public string Status { get; set; } = string.Empty;
        public string OrganiserName { get; set; } = string.Empty;
    }
}