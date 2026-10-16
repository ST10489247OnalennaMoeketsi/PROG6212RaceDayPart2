namespace RaceDay.API.Models
{
    /// <summary>
    /// Represents a reusable race category (e.g 5km, 10km., Half Marathon).
    /// </summary>
    public class EventCategory
    {
        // Composite Primary Key (EventId + CategoryId) will be configured in DbContext
        public int EventId { get; set; }
        public Event? Event { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
